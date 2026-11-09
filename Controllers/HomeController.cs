using CoffeeShopMVC.Data;
using CoffeeShopMVC.Models;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

public class HomeController : Controller
{
    private readonly CoffeeDbContext _db;

    public HomeController(CoffeeDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        await TrackVisitAsync();
        var vm = new HomeViewModel
        {
            BestSellers = await _db.Products.Include(p => p.Category)
                .Where(p => p.Status == "Available" && p.IsBestSeller)
                .Take(8).ToListAsync(),
            Trending = await _db.Products.Include(p => p.Category)
                .Where(p => p.Status == "Available" && p.IsTrending)
                .Take(8).ToListAsync(),
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync(),
            LatestBlogs = await _db.Blogs.OrderByDescending(b => b.CreatedAt).Take(3).ToListAsync()
        };
        return View(vm);
    }

    public IActionResult About() => View();
    public IActionResult Promo() => View();

    public IActionResult Error(int? statusCode = null)
    {
        ViewBag.StatusCode = statusCode;
        return View();
    }

    private async Task TrackVisitAsync()
    {
        try
        {
            _db.WebsiteVisits.Add(new WebsiteVisit
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                VisitDate = DateTime.Today,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Silently continue if telemetry tracking encounters transient database issues
        }
    }
}
