using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopMVC.Models;

public class Product
{
    private const decimal SmallSizeDiscount = 5000m;
    private const decimal LargeSizeSurcharge = 10000m;

    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000000)]
    public decimal Price { get; set; }

    public decimal GetPriceForSize(string? size)
    {
        var adjustment = size?.ToUpperInvariant() switch
        {
            "S" => -SmallSizeDiscount,
            "L" => LargeSizeSurcharge,
            _ => 0m
        };

        return Math.Round(Math.Max(0m, Price + adjustment), 0, MidpointRounding.AwayFromZero);
    }

    public decimal GetToppingPrice(string? topping)
    {
        return topping?.Trim() switch
        {
            "Trân châu" => 5000m,
            "Kem cheese" => 10000m,
            "Thạch cà phê" => 5000m,
            _ => 0m
        };
    }

    public decimal GetPriceForSizeAndTopping(string? size, string? topping)
    {
        return GetPriceForSize(size) + GetToppingPrice(topping);
    }

    public static bool IsValidSize(string? size)
    {
        return size is not null && new[] { "S", "M", "L" }.Contains(size, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsValidTopping(string? topping)
    {
        return string.IsNullOrWhiteSpace(topping)
            || new[] { "Trân châu", "Kem cheese", "Thạch cà phê" }.Contains(topping.Trim(), StringComparer.Ordinal);
    }

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
