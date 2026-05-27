using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopMVC.Controllers;

public class ContactController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(string fullName, string email, string content)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ thông tin liên hệ";
            return View();
        }
        TempData["Success"] = "Đã gửi liên hệ thành công";
        return RedirectToAction(nameof(Index));
    }
}
