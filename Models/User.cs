using System.ComponentModel.DataAnnotations;

namespace CoffeeShopMVC.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Member";

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";

    [MaxLength(300)]
    public string? AvatarUrl { get; set; } = "/images/default-avatar.svg";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
