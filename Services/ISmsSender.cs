namespace CoffeeShopMVC.Services;

public interface ISmsSender
{
    bool IsConfigured { get; }
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}