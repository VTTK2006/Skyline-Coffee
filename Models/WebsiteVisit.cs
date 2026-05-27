using System.ComponentModel.DataAnnotations;

namespace CoffeeShopMVC.Models;

public class WebsiteVisit
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string? IpAddress { get; set; }

    [MaxLength(300)]
    public string? UserAgent { get; set; }

    public DateTime VisitDate { get; set; } = DateTime.Today;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
