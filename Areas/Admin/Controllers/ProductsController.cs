using CoffeeShopMVC.Data;
using CoffeeShopMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly CoffeeDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(CoffeeDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, string? status)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search.Trim()));
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Status = status;
        return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(new Product { Status = "Available" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (imageFile != null)
        {
            var result = await SaveProductImageAsync(imageFile);
            if (!result.Success) ModelState.AddModelError("ImageUrl", result.Message);
            else product.ImageUrl = result.Path;
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(product);
        }

        product.CreatedAt = DateTime.Now;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm sản phẩm";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product input, IFormFile? imageFile)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(input);
        }

        product.Name = input.Name;
        product.Price = input.Price;
        product.Description = input.Description;
        product.CategoryId = input.CategoryId;
        product.Status = input.Status;
        product.IsBestSeller = input.IsBestSeller;
        product.IsTrending = input.IsTrending;
        product.UpdatedAt = DateTime.Now;

        if (imageFile != null)
        {
            var result = await SaveProductImageAsync(imageFile);
            if (!result.Success)
            {
                ModelState.AddModelError("ImageUrl", result.Message);
                ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                return View(input);
            }
            product.ImageUrl = result.Path;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật sản phẩm";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        var hasOrders = await _db.OrderDetails.AnyAsync(od => od.ProductId == id);
        if (hasOrders)
        {
            product.Status = "Unavailable";
            product.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Món này đã có trong đơn hàng của khách. Đã cập nhật trạng thái thành 'Hết hàng' để lưu giữ lịch sử đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        var cartItems = await _db.CartItems.Where(c => c.ProductId == id).ToListAsync();
        if (cartItems.Any())
        {
            _db.CartItems.RemoveRange(cartItems);
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa sản phẩm thành công";
        return RedirectToAction(nameof(Index));
    }

    private async Task<(bool Success, string Message, string? Path)> SaveProductImageAsync(IFormFile file)
    {
        var allowed = new[] { ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext)) return (false, "Chỉ cho phép ảnh .jpg, .jpeg, .png", null);
        if (file.Length > 5 * 1024 * 1024) return (false, "Ảnh sản phẩm tối đa 5MB", null);

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return (true, "OK", $"/uploads/products/{fileName}");
    }
}
