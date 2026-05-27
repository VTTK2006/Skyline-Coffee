using System.ComponentModel.DataAnnotations;

namespace CoffeeShopMVC.Models;

public class Blog
{
    public int Id { get; set; }

    [Required, MaxLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
