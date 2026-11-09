using CoffeeShopMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly CoffeeDbContext _db;
    public OrdersController(CoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, string? status, DateTime? date)
    {
        var query = _db.Orders.Include(o => o.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.Id.ToString().Contains(search) || o.FullName.Contains(search) || o.Phone.Contains(search));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);
        if (date.HasValue) query = query.Where(o => o.CreatedAt.Date == date.Value.Date);
        ViewBag.Status = status;
        ViewBag.Search = search;
        ViewBag.Date = date?.ToString("yyyy-MM-dd");
        return View(await query.OrderByDescending(o => o.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders.Include(o => o.User).Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowed = new[] { "Chờ thanh toán", "Chờ xử lý", "Đang chuẩn bị", "Đang giao", "Hoàn thành", "Đã hủy" };
        if (!allowed.Contains(status)) return BadRequest();
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        if (order.PaymentMethod == "VNPay" && order.Status == "Chờ thanh toán" && status != "Đã hủy")
            return BadRequest();

        order.Status = status;
        order.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái đơn hàng";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmVnpayManually(int id)
    {
        var order = await _db.Orders.Include(item => item.OrderDetails).FirstOrDefaultAsync(item =>
            item.Id == id && item.PaymentMethod == "VNPay" && item.Status == "Chờ thanh toán");
        if (order == null) return NotFound();

        foreach (var detail in order.OrderDetails)
        {
            var cartItem = await _db.CartItems.FirstOrDefaultAsync(item =>
                item.UserId == order.UserId
                && item.ProductId == detail.ProductId
                && item.Size == detail.Size
                && item.Topping == detail.Topping);
            if (cartItem == null) continue;

            cartItem.Quantity -= detail.Quantity;
            if (cartItem.Quantity <= 0)
                _db.CartItems.Remove(cartItem);
        }

        order.Status = "Chờ xử lý";
        order.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã xác nhận thanh toán VNPay thủ công; đơn hàng chuyển sang chờ xử lý.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
