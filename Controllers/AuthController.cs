using System.Security.Claims;
using CoffeeShopMVC.Data;
using CoffeeShopMVC.Models;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

public class AuthController : Controller
{
    private readonly CoffeeDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _config;

    public AuthController(CoffeeDbContext db, IPasswordHasher<User> hasher, IConfiguration config)
    {
        _db = db;
        _hasher = hasher;
        _config = config;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var loginInput = model.EmailOrUsername?.Trim() ?? string.Empty;
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email == loginInput || u.Username == loginInput);

        if (user == null || _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng");
            return View(model);
        }

        if (user.Status == "Blocked")
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị chặn. Vui lòng liên hệ quản trị viên.");
            return View(model);
        }

        await SignInUserAsync(user, model.RememberMe);
        TempData["Success"] = "Đăng nhập thành công";

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) 
            && Url.IsLocalUrl(model.ReturnUrl)
            && !model.ReturnUrl.Contains("/Add", StringComparison.OrdinalIgnoreCase)
            && !model.ReturnUrl.Contains("/Update", StringComparison.OrdinalIgnoreCase)
            && !model.ReturnUrl.Contains("/Remove", StringComparison.OrdinalIgnoreCase)
            && !model.ReturnUrl.Contains("/Delete", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(model.ReturnUrl);
        }

        return user.Role == "Admin" ? RedirectToAction("Index", "Dashboard", new { area = "Admin" }) : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var username = model.Username?.Trim() ?? string.Empty;

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng");
            return View(model);
        }

        if (await _db.Users.AnyAsync(u => u.Username == username))
        {
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã tồn tại");
            return View(model);
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Phone = model.Phone?.Trim(),
            Role = "Member",
            Status = "Active",
            AvatarUrl = "/images/default-avatar.svg",
            CreatedAt = DateTime.Now
        };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInUserAsync(user, false);
        TempData["Success"] = "Đăng ký tài khoản thành công";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Đã đăng xuất";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    // External Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SignIn(string provider)
    {
        var clientId = _config[$"Authentication:{provider}:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId) || clientId.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = $"Chức năng đăng nhập qua {provider} hiện chưa được thiết lập API Key trong cấu hình hệ thống.";
            return RedirectToAction("Login");
        }

        var redirectUrl = Url.Action("ExternalLoginCallback", "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded || result.Principal == null)
        {
            TempData["Error"] = "Đăng nhập bên ngoài không thành công hoặc đã bị hủy.";
            return RedirectToAction("Login");
        }

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
        var provider = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Issuer ?? "external";

        if (string.IsNullOrEmpty(email))
        {
            email = string.IsNullOrEmpty(name) 
                ? $"user{Guid.NewGuid().ToString("N")[..8]}@{provider.ToLowerInvariant()}.com"
                : $"{name.Replace(" ", "").ToLowerInvariant()}@{provider.ToLowerInvariant()}.com";
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            var baseUsername = string.IsNullOrEmpty(name) ? email.Split('@')[0] : name;
            var finalUsername = baseUsername;
            var counter = 1;
            while (await _db.Users.AnyAsync(u => u.Username == finalUsername))
            {
                finalUsername = $"{baseUsername}{counter++}";
            }

            user = new User
            {
                Email = email,
                Username = finalUsername,
                Role = "Member",
                Status = "Active",
                AvatarUrl = "/images/default-avatar.svg",
                CreatedAt = DateTime.Now
            };
            user.PasswordHash = _hasher.HashPassword(user, Guid.NewGuid().ToString("N"));
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        if (user.Status == "Blocked")
        {
            await HttpContext.SignOutAsync("External");
            TempData["Error"] = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.";
            return RedirectToAction("Login");
        }

        await SignInUserAsync(user, false);
        await HttpContext.SignOutAsync("External");
        TempData["Success"] = "Đăng nhập thành công";

        if (!string.IsNullOrWhiteSpace(returnUrl) 
            && Url.IsLocalUrl(returnUrl)
            && !returnUrl.Contains("/Add", StringComparison.OrdinalIgnoreCase)
            && !returnUrl.Contains("/Update", StringComparison.OrdinalIgnoreCase)
            && !returnUrl.Contains("/Remove", StringComparison.OrdinalIgnoreCase)
            && !returnUrl.Contains("/Delete", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(returnUrl);
        }

        return user.Role == "Admin" ? RedirectToAction("Index", "Dashboard", new { area = "Admin" }) : RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(User user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.svg")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = rememberMe });
    }
}
