using CoffeeShopMVC.Models;

namespace CoffeeShopMVC.ViewModels;

public class DashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int NewOrders { get; set; }
    public int TodayVisits { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalSoldItems { get; set; }
    public List<ChartPoint> RevenueByDay { get; set; } = new();
    public List<ChartPoint> OrdersByDay { get; set; } = new();
    public List<ChartPoint> TopProducts { get; set; } = new();
    public List<ChartPoint> OrderStatusChart { get; set; } = new();
    public List<Order> LatestOrders { get; set; } = new();
}

public class ChartPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
