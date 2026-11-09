using CoffeeShopMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

public class ProductController : Controller
{
    private readonly CoffeeDbContext _db;

    public ProductController(CoffeeDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var query = _db.Products.Include(p => p.Category)
            .Where(p => p.Status == "Available")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p => p.Name.Contains(search));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
        {
            (minPrice, maxPrice) = (maxPrice, minPrice);
        }

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    public async Task<IActionResult> BestSeller()
    {
        ViewBag.Title = "Best Seller";
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        var products = await _db.Products.Include(p => p.Category)
            .Where(p => p.Status == "Available" && p.IsBestSeller)
            .ToListAsync();
        return View("Index", products);
    }

    public async Task<IActionResult> Trending()
    {
        ViewBag.Title = "Trending";
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        var products = await _db.Products.Include(p => p.Category)
            .Where(p => p.Status == "Available" && p.IsTrending)
            .ToListAsync();
        return View("Index", products);
    }
}
