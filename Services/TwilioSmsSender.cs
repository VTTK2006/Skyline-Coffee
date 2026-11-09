using System.Net.Http.Headers;
using System.Text;

namespace CoffeeShopMVC.Services;

public sealed class TwilioSmsSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromNumber;

    public TwilioSmsSender(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _accountSid = configuration["Sms:Twilio:AccountSid"];
        _authToken = configuration["Sms:Twilio:AuthToken"];
        _fromNumber = configuration["Sms:Twilio:FromNumber"];
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_accountSid)
        && !string.IsNullOrWhiteSpace(_authToken)
        && !string.IsNullOrWhiteSpace(_fromNumber);

    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured) throw new InvalidOperationException("Twilio SMS is not configured.");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_accountSid}:{_authToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = phoneNumber,
            ["From"] = _fromNumber!,
            ["Body"] = message
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}