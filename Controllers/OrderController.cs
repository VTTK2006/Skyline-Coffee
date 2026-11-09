using CoffeeShopMVC.Data;
using CoffeeShopMVC.Extensions;
using CoffeeShopMVC.Models;
using CoffeeShopMVC.Services;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly CoffeeDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly VnpayPaymentService _vnpay;

    public OrderController(CoffeeDbContext db, IConfiguration configuration, VnpayPaymentService vnpay)
    {
        _db = db;
        _configuration = configuration;
        _vnpay = vnpay;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var userId = User.GetUserId();
        var cartItems = await _db.CartItems.Include(c => c.Product).Where(c => c.UserId == userId).ToListAsync();
        if (!cartItems.Any())
        {
            TempData["Error"] = "Giỏ hàng đang trống";
            return RedirectToAction("Index", "Cart");
        }

        var user = await _db.Users.FindAsync(userId);
        var defaultAddress = await _db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
        var vm = new CheckoutViewModel
        {
            FullName = user?.Username ?? string.Empty,
            Phone = defaultAddress?.Phone ?? user?.Phone ?? string.Empty,
            Address = defaultAddress?.AddressDetail ?? string.Empty,
            CartItems = cartItems
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var userId = User.GetUserId();
        var cartItems = await _db.CartItems.Include(c => c.Product).Where(c => c.UserId == userId).ToListAsync();
        model.CartItems = cartItems;

        if (!cartItems.Any())
        {
            TempData["Error"] = "Giỏ hàng đang trống";
            return RedirectToAction("Index", "Cart");
        }

        var isVnpay = model.PaymentMethod == "VNPay";
        if (isVnpay && !_vnpay.IsConfigured)
        {
            ModelState.AddModelError(string.Empty, "Thanh toán VNPay chưa được cấu hình đầy đủ.");
            return View(model);
        }

        if (!ModelState.IsValid) return View(model);

        if (isVnpay)
        {
            var pendingOrder = await _db.Orders.FirstOrDefaultAsync(order =>
                order.UserId == userId
                && order.PaymentMethod == "VNPay"
                && order.Status == "Chờ thanh toán");
            if (pendingOrder != null)
            {
                TempData["Error"] = "Bạn đang có đơn VNPay chờ thanh toán. Hãy hoàn tất hoặc hủy đơn đó trước.";
                return RedirectToAction(nameof(Success), new { id = pendingOrder.Id });
            }
        }

        var order = new Order
        {
            UserId = userId,
            FullName = model.FullName,
            Phone = model.Phone,
            Address = model.Address,
            Note = model.Note,
            PaymentMethod = model.PaymentMethod,
            Status = isVnpay ? "Chờ thanh toán" : "Chờ xử lý",
            CreatedAt = DateTime.Now,
            TotalPrice = cartItems.Sum(i => (i.Product?.GetPriceForSizeAndTopping(i.Size, i.Topping) ?? 0) * i.Quantity)
        };

        foreach (var item in cartItems)
        {
            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Sản phẩm",
                Quantity = item.Quantity,
                Price = item.Product?.GetPriceForSizeAndTopping(item.Size, item.Topping) ?? 0,
                Size = item.Size,
                Topping = item.Topping,
                CreatedAt = DateTime.Now
            });
        }

        _db.Orders.Add(order);
        if (!isVnpay)
            _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync();

        if (isVnpay)
            return Redirect(CreateVnpayPaymentUrl(order));

        TempData["Success"] = "Đặt hàng thành công";
        return RedirectToAction("Success", new { id = order.Id });
    }

    public async Task<IActionResult> Success(int id)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == User.GetUserId());
        if (order == null) return NotFound();

        ViewBag.IsBankTransfer = order.PaymentMethod == "Chuyển khoản";
        ViewBag.IsVnpay = order.PaymentMethod == "VNPay";
        ViewBag.VnpayResult = TempData["VnpayResult"]?.ToString();
        var bankBin = _configuration["Payments:VietQr:BankBin"]?.Trim();
        var accountNumber = _configuration["Payments:VietQr:AccountNumber"]?.Trim();
        var accountName = _configuration["Payments:VietQr:AccountName"]?.Trim();

        if (ViewBag.IsBankTransfer && !string.IsNullOrWhiteSpace(bankBin)
            && !string.IsNullOrWhiteSpace(accountNumber) && !string.IsNullOrWhiteSpace(accountName))
        {
            var transferInfo = Uri.EscapeDataString($"SKYLINE {order.Id}");
            var encodedAccountName = Uri.EscapeDataString(accountName!);
            ViewBag.BankQrUrl = $"https://img.vietqr.io/image/{Uri.EscapeDataString(bankBin!)}-{Uri.EscapeDataString(accountNumber!)}-compact2.png?amount={order.TotalPrice:0}&addInfo={transferInfo}&accountName={encodedAccountName}";
            ViewBag.BankAccountNumber = accountNumber;
            ViewBag.BankAccountName = accountName;
        }

        return View(order);
    }

    private string CreateVnpayPaymentUrl(Order order)
    {
        var publicBaseUrl = _configuration["Vnpay:PublicBaseUrl"]?.TrimEnd('/');
        var returnUrl = string.IsNullOrWhiteSpace(publicBaseUrl)
            ? Url.Action("Return", "Vnpay", values: null, protocol: Request.Scheme)!
            : $"{publicBaseUrl}/Vnpay/Return";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        return _vnpay.CreatePaymentUrl(order.Id, order.TotalPrice, ipAddress, returnUrl);
    }
}
