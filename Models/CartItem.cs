using System.ComponentModel.DataAnnotations;

namespace CoffeeShopMVC.Models;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    [MaxLength(10)]
    public string Size { get; set; } = "M";

    [MaxLength(120)]
    public string? Topping { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
