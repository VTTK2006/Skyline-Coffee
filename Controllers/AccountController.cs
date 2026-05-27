using System.Security.Claims;
using CoffeeShopMVC.Data;
using CoffeeShopMVC.Extensions;
using CoffeeShopMVC.Models;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly CoffeeDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IWebHostEnvironment _env;

    public AccountController(CoffeeDbContext db, IPasswordHasher<User> hasher, IWebHostEnvironment env)
    {
        _db = db;
        _hasher = hasher;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user == null) return NotFound();
        return View(new ProfileViewModel
        {
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user == null) return NotFound();

        if (await _db.Users.AnyAsync(u => u.Username == model.Username && u.Id != user.Id))
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã tồn tại");

        if (!ModelState.IsValid)
        {
            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;
            model.CreatedAt = user.CreatedAt;
            return View(model);
        }

        user.Username = model.Username;
        user.Phone = model.Phone;
        user.UpdatedAt = DateTime.Now;

        if (model.AvatarFile != null)
        {
            var uploadResult = await SaveImageAsync(model.AvatarFile, "avatars", 2);
            if (!uploadResult.Success)
            {
                ModelState.AddModelError(nameof(model.AvatarFile), uploadResult.Message);
                model.Email = user.Email;
                model.AvatarUrl = user.AvatarUrl;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }
            user.AvatarUrl = uploadResult.Path;
        }

        await _db.SaveChangesAsync();
        await RefreshLoginAsync(user);
        TempData["Success"] = "Cập nhật tài khoản thành công";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user == null) return NotFound();

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.OldPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu cũ không đúng");
            return View(model);
        }

        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đổi mật khẩu thành công";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> Addresses()
    {
        var userId = User.GetUserId();
        var addresses = await _db.Addresses.Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ToListAsync();
        return View(addresses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(Address address)
    {
        var userId = User.GetUserId();
        address.UserId = userId;
        address.CreatedAt = DateTime.Now;
        if (address.IsDefault)
        {
            var oldDefaults = _db.Addresses.Where(a => a.UserId == userId);
            await oldDefaults.ForEachAsync(a => a.IsDefault = false);
        }
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm địa chỉ";
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = User.GetUserId();
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address != null)
        {
            _db.Addresses.Remove(address);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var userId = User.GetUserId();
        var addresses = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
        foreach (var address in addresses) address.IsDefault = address.Id == id;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Addresses));
    }

    public async Task<IActionResult> MyOrders()
    {
        var userId = User.GetUserId();
        var orders = await _db.Orders.Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt).ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> OrderDetails(int id)
    {
        var userId = User.GetUserId();
        var order = await _db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = User.GetUserId();
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();
        if (order.Status == "Chờ xử lý")
        {
            order.Status = "Đã hủy";
            order.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã hủy đơn hàng";
        }
        else
        {
            TempData["Error"] = "Không thể hủy đơn hàng ở trạng thái hiện tại";
        }
        return RedirectToAction(nameof(MyOrders));
    }

    private async Task<(bool Success, string Message, string? Path)> SaveImageAsync(IFormFile file, string folder, int maxMb)
    {
        var allowed = new[] { ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext)) return (false, "Chỉ cho phép ảnh .jpg, .jpeg, .png", null);
        if (file.Length > maxMb * 1024 * 1024) return (false, $"Ảnh tối đa {maxMb}MB", null);

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return (true, "OK", $"/uploads/{folder}/{fileName}");
    }

    private async Task RefreshLoginAsync(User user)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.svg")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
