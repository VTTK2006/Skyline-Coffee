using CoffeeShopMVC.Data;
using CoffeeShopMVC.Extensions;
using CoffeeShopMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly CoffeeDbContext _db;

    public CartController(CoffeeDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var items = await _db.CartItems.Include(c => c.Product)
            .ThenInclude(p => p!.Category)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Add() => RedirectToAction("Index");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string size = "M", string? topping = null)
    {
        var userId = User.GetUserId();
        var product = await _db.Products.FindAsync(productId);
        if (product == null || product.Status != "Available") return NotFound();

        var qty = Math.Max(1, quantity);
        var existing = await _db.CartItems.FirstOrDefaultAsync(c =>
            c.UserId == userId && c.ProductId == productId && c.Size == size && c.Topping == topping);

        if (existing != null)
        {
            existing.Quantity += qty;
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = qty,
                Size = size,
                Topping = topping,
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm món vào giỏ hàng";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Update() => RedirectToAction("Index");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        var userId = User.GetUserId();
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item == null) return NotFound();

        if (quantity <= 0)
        {
            _db.CartItems.Remove(item);
            TempData["Success"] = "Đã xóa món khỏi giỏ hàng";
        }
        else
        {
            item.Quantity = quantity;
            TempData["Success"] = "Đã cập nhật số lượng món";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Remove() => RedirectToAction("Index");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = User.GetUserId();
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa món khỏi giỏ hàng";
        }
        return RedirectToAction("Index");
    }
}
