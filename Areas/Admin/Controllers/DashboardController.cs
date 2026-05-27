using CoffeeShopMVC.Data;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly CoffeeDbContext _db;
    public DashboardController(CoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
        var completed = _db.Orders.Where(o => o.Status == "Hoàn thành");

        var vm = new DashboardViewModel
        {
            TotalRevenue = await completed.SumAsync(o => (decimal?)o.TotalPrice) ?? 0,
            TodayOrders = await _db.Orders.CountAsync(o => o.CreatedAt.Date == today),
            NewOrders = await _db.Orders.CountAsync(o => o.Status == "Chờ xử lý"),
            TodayVisits = await _db.WebsiteVisits.CountAsync(v => v.VisitDate.Date == today),
            TotalCustomers = await _db.Users.CountAsync(u => u.Role == "Member"),
            TotalProducts = await _db.Products.CountAsync(p => p.Status == "Available"),
            TotalSoldItems = await _db.OrderDetails.Where(od => od.Order != null && od.Order.Status == "Hoàn thành").SumAsync(od => (int?)od.Quantity) ?? 0,
            LatestOrders = await _db.Orders.Include(o => o.User).OrderByDescending(o => o.CreatedAt).Take(10).ToListAsync()
        };

        foreach (var day in last7Days)
        {
            vm.RevenueByDay.Add(new ChartPoint
            {
                Label = day.ToString("dd/MM"),
                Value = await _db.Orders.Where(o => o.Status == "Hoàn thành" && o.CreatedAt.Date == day).SumAsync(o => (decimal?)o.TotalPrice) ?? 0
            });
            vm.OrdersByDay.Add(new ChartPoint
            {
                Label = day.ToString("dd/MM"),
                Value = await _db.Orders.CountAsync(o => o.CreatedAt.Date == day)
            });
        }

        vm.TopProducts = await _db.OrderDetails
            .Where(od => od.Order != null && od.Order.Status == "Hoàn thành")
            .GroupBy(od => od.ProductName)
            .Select(g => new ChartPoint { Label = g.Key, Value = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Value)
            .Take(8).ToListAsync();

        return View(vm);
    }
}
