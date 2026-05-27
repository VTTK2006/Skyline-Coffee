using CoffeeShopMVC.Models;

namespace CoffeeShopMVC.ViewModels;

public class HomeViewModel
{
    public List<Product> BestSellers { get; set; } = new();
    public List<Product> Trending { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Blog> LatestBlogs { get; set; } = new();
}
