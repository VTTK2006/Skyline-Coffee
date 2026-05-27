using CoffeeShopMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

public class BlogController : Controller
{
    private readonly CoffeeDbContext _db;
    public BlogController(CoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var blogs = await _db.Blogs.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(blogs);
    }

    public async Task<IActionResult> Details(int id)
    {
        var blog = await _db.Blogs.FindAsync(id);
        if (blog == null) return NotFound();
        return View(blog);
    }
}
