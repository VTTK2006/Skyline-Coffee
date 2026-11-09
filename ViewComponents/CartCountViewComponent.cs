using CoffeeShopMVC.Data;
using CoffeeShopMVC.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.ViewComponents;

public sealed class CartCountViewComponent : ViewComponent
{
    private readonly CoffeeDbContext _db;

    public CartCountViewComponent(CoffeeDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (HttpContext.User.Identity?.IsAuthenticated != true) return Content(string.Empty);

        var userId = HttpContext.User.GetUserId();
        if (userId <= 0) return Content(string.Empty);

        var count = await _db.CartItems
            .Where(item => item.UserId == userId)
            .SumAsync(item => (int?)item.Quantity) ?? 0;

        return count > 0 ? View(count) : Content(string.Empty);
    }
}