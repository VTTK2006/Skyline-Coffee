using System.ComponentModel.DataAnnotations;

namespace CoffeeShopMVC.Models;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string AddressDetail { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
