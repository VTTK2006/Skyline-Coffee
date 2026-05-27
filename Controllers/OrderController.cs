using CoffeeShopMVC.Data;
using CoffeeShopMVC.Extensions;
using CoffeeShopMVC.Models;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly CoffeeDbContext _db;

    public OrderController(CoffeeDbContext db)
    {
        _db = db;
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

        if (!ModelState.IsValid) return View(model);

        var order = new Order
        {
            UserId = userId,
            FullName = model.FullName,
            Phone = model.Phone,
            Address = model.Address,
            Note = model.Note,
            PaymentMethod = model.PaymentMethod,
            Status = "Chờ xử lý",
            CreatedAt = DateTime.Now,
            TotalPrice = cartItems.Sum(i => (i.Product?.Price ?? 0) * i.Quantity)
        };

        foreach (var item in cartItems)
        {
            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Sản phẩm",
                Quantity = item.Quantity,
                Price = item.Product?.Price ?? 0,
                Size = item.Size,
                Topping = item.Topping,
                CreatedAt = DateTime.Now
            });
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đặt hàng thành công";
        return RedirectToAction("Success", new { id = order.Id });
    }

    public IActionResult Success(int id)
    {
        ViewBag.OrderId = id;
        return View();
    }
}
