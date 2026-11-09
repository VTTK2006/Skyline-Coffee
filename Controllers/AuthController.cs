using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using CoffeeShopMVC.Data;
using CoffeeShopMVC.Models;
using CoffeeShopMVC.Services;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoffeeShopMVC.Controllers;

public class AuthController : Controller
{
    private readonly CoffeeDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly ISmsSender _smsSender;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        CoffeeDbContext db,
        IPasswordHasher<User> hasher,
        IConfiguration config,
        IMemoryCache cache,
        ISmsSender smsSender,
        ILogger<AuthController> logger)
    {
        _db = db;
        _hasher = hasher;
        _config = config;
        _cache = cache;
        _smsSender = smsSender;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpGet]
    public IActionResult ForgotPassword(string? phone = null)
    {
        return View(new ForgotPasswordViewModel { Phone = phone ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (!_smsSender.IsConfigured)
        {
            ModelState.AddModelError(string.Empty, "Chức năng SMS chưa được cấu hình. Vui lòng liên hệ quản trị viên.");
            return View(model);
        }

        var normalizedPhone = NormalizeVietnamesePhone(model.Phone);
        if (normalizedPhone == null)
        {
            ModelState.AddModelError(nameof(model.Phone), "Vui lòng nhập số điện thoại Việt Nam hợp lệ.");
            return View(model);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (IsRateLimited($"password-reset-ip:{ipAddress}", 10, TimeSpan.FromMinutes(15)))
        {
            ModelState.AddModelError(string.Empty, "Bạn đã yêu cầu quá nhiều mã. Vui lòng thử lại sau.");
            return View(model);
        }

        var phoneCacheKey = $"password-reset-phone:{normalizedPhone}";
        if (_cache.TryGetValue(phoneCacheKey, out _))
        {
            return View("VerifyOtp", new VerifyPasswordResetViewModel { Phone = model.Phone.Trim() });
        }
        _cache.Set(phoneCacheKey, true, TimeSpan.FromSeconds(60));

        var usersWithPhone = await _db.Users
            .Where(user => user.Phone != null)
            .Select(user => new { user.Id, user.Phone })
            .ToListAsync();
        var matchingUsers = usersWithPhone
            .Where(user => NormalizeVietnamesePhone(user.Phone) == normalizedPhone)
            .ToArray();

        if (matchingUsers.Length == 1)
        {
            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var salt = RandomNumberGenerator.GetBytes(16);
            var otpKey = $"password-reset-otp:{normalizedPhone}";
            _cache.Set(otpKey, new PasswordResetOtp
            {
                UserId = matchingUsers[0].Id,
                Salt = salt,
                Hash = HashOtp(salt, otp)
            }, TimeSpan.FromMinutes(5));

            try
            {
                await _smsSender.SendAsync($"+{normalizedPhone}", $"Ma xac minh SkyLine Coffee: {otp}. Ma het han sau 5 phut.");
            }
            catch (Exception exception)
            {
                _cache.Remove(otpKey);
                _logger.LogError(exception, "Unable to send password reset SMS");
            }
        }

        TempData["Success"] = "Nếu số điện thoại thuộc tài khoản, mã OTP đã được gửi và có hiệu lực trong 5 phút.";
        return View("VerifyOtp", new VerifyPasswordResetViewModel { Phone = model.Phone.Trim() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyPasswordResetViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var normalizedPhone = NormalizeVietnamesePhone(model.Phone);
        if (normalizedPhone == null)
        {
            ModelState.AddModelError(nameof(model.Phone), "Số điện thoại không hợp lệ.");
            return View(model);
        }

        var otpKey = $"password-reset-otp:{normalizedPhone}";
        if (!_cache.TryGetValue<PasswordResetOtp>(otpKey, out var resetOtp) || resetOtp == null)
        {
            ModelState.AddModelError(string.Empty, "Mã OTP không đúng hoặc đã hết hạn. Hãy yêu cầu mã mới.");
            return View(model);
        }

        var providedHash = HashOtp(resetOtp.Salt, model.Otp.Trim());
        if (!CryptographicOperations.FixedTimeEquals(resetOtp.Hash, providedHash))
        {
            resetOtp.FailedAttempts++;
            if (resetOtp.FailedAttempts >= 5)
            {
                _cache.Remove(otpKey);
                ModelState.AddModelError(string.Empty, "Bạn đã nhập sai quá số lần cho phép. Hãy yêu cầu mã OTP mới.");
            }
            else
            {
                ModelState.AddModelError(nameof(model.Otp), "Mã OTP không chính xác.");
            }

            return View(model);
        }

        var user = await _db.Users.FindAsync(resetOtp.UserId);
        if (user == null || NormalizeVietnamesePhone(user.Phone) != normalizedPhone)
        {
            _cache.Remove(otpKey);
            ModelState.AddModelError(string.Empty, "Không thể xác minh tài khoản. Hãy yêu cầu mã mới.");
            return View(model);
        }

        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        _cache.Remove(otpKey);
        _cache.Remove($"password-reset-phone:{normalizedPhone}");

        TempData["Success"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
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

    private bool IsRateLimited(string key, int limit, TimeSpan window)
    {
        var attempts = _cache.Get<int>(key);
        if (attempts >= limit) return true;

        _cache.Set(key, attempts + 1, window);
        return false;
    }

    private static string? NormalizeVietnamesePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0084", StringComparison.Ordinal)) digits = digits[2..];
        if (digits.StartsWith('0')) digits = $"84{digits[1..]}";
        if (!digits.StartsWith("84", StringComparison.Ordinal) || digits.Length is < 11 or > 12) return null;

        return digits;
    }

    private static byte[] HashOtp(byte[] salt, string otp)
    {
        var otpBytes = Encoding.ASCII.GetBytes(otp);
        var input = new byte[salt.Length + otpBytes.Length];
        Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
        Buffer.BlockCopy(otpBytes, 0, input, salt.Length, otpBytes.Length);
        return SHA256.HashData(input);
    }

    private sealed class PasswordResetOtp
    {
        public int UserId { get; init; }
        public byte[] Salt { get; init; } = Array.Empty<byte>();
        public byte[] Hash { get; init; } = Array.Empty<byte>();
        public int FailedAttempts { get; set; }
    }
}
