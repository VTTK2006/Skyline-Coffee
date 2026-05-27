using CoffeeShopMVC.Data;
using CoffeeShopMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class StatisticsController : Controller
{
    private readonly CoffeeDbContext _db;
    public StatisticsController(CoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var days = Enumerable.Range(0, 30).Select(i => today.AddDays(-29 + i)).ToList();
        var vm = new DashboardViewModel
        {
            TotalRevenue = await _db.Orders.Where(o => o.Status == "Hoàn thành").SumAsync(o => (decimal?)o.TotalPrice) ?? 0,
            TodayOrders = await _db.Orders.CountAsync(o => o.CreatedAt.Date == today),
            NewOrders = await _db.Orders.CountAsync(o => o.Status == "Chờ xử lý"),
            TotalCustomers = await _db.Users.CountAsync(u => u.Role == "Member"),
            TotalProducts = await _db.Products.CountAsync(p => p.Status == "Available"),
            TotalSoldItems = await _db.OrderDetails.Where(od => od.Order != null && od.Order.Status == "Hoàn thành").SumAsync(od => (int?)od.Quantity) ?? 0
        };

        foreach (var day in days)
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
            .Take(10).ToListAsync();

        vm.OrderStatusChart = await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new ChartPoint { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToListAsync();

        return View(vm);
    }
}
