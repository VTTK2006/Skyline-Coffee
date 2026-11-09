using System.ComponentModel.DataAnnotations;
using CoffeeShopMVC.Models;

namespace CoffeeShopMVC.ViewModels;

public class CheckoutViewModel
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }

    [Required]
    [RegularExpression("^(Thanh toán khi nhận hàng|VNPay)$")]
    public string PaymentMethod { get; set; } = "Thanh toán khi nhận hàng";

    public List<CartItem> CartItems { get; set; } = new();
    public decimal TotalPrice => CartItems.Sum(i => (i.Product?.GetPriceForSizeAndTopping(i.Size, i.Topping) ?? 0) * i.Quantity);
}
