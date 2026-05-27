using CoffeeShopMVC.Data;
using CoffeeShopMVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly CoffeeDbContext _db;
    public UsersController(CoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search) || (u.Phone != null && u.Phone.Contains(search)));
        ViewBag.Search = search;
        return View(await query.OrderByDescending(u => u.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _db.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id)
    {
        var currentId = User.GetUserId();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Id == currentId)
        {
            TempData["Error"] = "Không thể tự chặn tài khoản của chính mình";
            return RedirectToAction(nameof(Index));
        }
        user.Status = user.Status == "Blocked" ? "Active" : "Blocked";
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái tài khoản";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int id, string role)
    {
        if (role != "Admin" && role != "Member") return BadRequest();
        var currentId = User.GetUserId();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var adminCount = await _db.Users.CountAsync(u => u.Role == "Admin");
        if (user.Id == currentId && user.Role == "Admin" && role == "Member" && adminCount <= 1)
        {
            TempData["Error"] = "Không thể hạ quyền Admin cuối cùng trong hệ thống";
            return RedirectToAction(nameof(Index));
        }

        user.Role = role;
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật vai trò tài khoản";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentId = User.GetUserId();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Id == currentId)
        {
            TempData["Error"] = "Không thể tự xóa tài khoản đang đăng nhập";
            return RedirectToAction(nameof(Index));
        }
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa tài khoản";
        return RedirectToAction(nameof(Index));
    }
}
