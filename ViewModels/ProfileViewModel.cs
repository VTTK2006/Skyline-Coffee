using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CoffeeShopMVC.ViewModels;

public class ProfileViewModel
{
    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public IFormFile? AvatarFile { get; set; }
}
