using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopMVC.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000000)]
    public decimal Price { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Available";

    public bool IsBestSeller { get; set; }
    public bool IsTrending { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
