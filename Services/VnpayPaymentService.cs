using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace CoffeeShopMVC.Services;

public sealed class VnpayPaymentService
{
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _paymentUrl;
    private readonly string _bankCode;

    public VnpayPaymentService(IConfiguration configuration)
    {
        _tmnCode = configuration["Vnpay:TmnCode"]?.Trim() ?? string.Empty;
        _hashSecret = configuration["Vnpay:HashSecret"] ?? string.Empty;
        _paymentUrl = configuration["Vnpay:PaymentUrl"]?.Trim() ?? string.Empty;
        _bankCode = configuration["Vnpay:BankCode"]?.Trim() ?? string.Empty;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_tmnCode)
        && !string.IsNullOrWhiteSpace(_hashSecret)
        && Uri.TryCreate(_paymentUrl, UriKind.Absolute, out _);

    public string CreatePaymentUrl(
        int orderId,
        decimal amount,
        string ipAddress,
        string returnUrl)
    {
        if (!IsConfigured) throw new InvalidOperationException("VNPay is not configured.");
        if (amount <= 0 || decimal.Truncate(amount) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), "VNPay amount must be a positive whole number of VND.");

        var now = GetVietnamTime();
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _tmnCode,
            ["vnp_Amount"] = checked((long)(amount * 100m)).ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = orderId.ToString(CultureInfo.InvariantCulture),
            ["vnp_OrderInfo"] = $"Thanh toan don hang {orderId}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_IpAddr"] = ipAddress,
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrWhiteSpace(_bankCode))
            parameters["vnp_BankCode"] = _bankCode;

        var signData = BuildQuery(parameters);
        var signature = ComputeSignature(signData);
        return $"{_paymentUrl}?{signData}&vnp_SecureHash={signature}";
    }

    public bool TryValidateResponse(IQueryCollection query, out SortedDictionary<string, string> parameters)
    {
        parameters = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var providedSignature = query["vnp_SecureHash"].ToString();
        if (string.IsNullOrWhiteSpace(providedSignature)) return false;

        foreach (var key in query.Keys.Where(key =>
                     key.StartsWith("vnp_", StringComparison.Ordinal)
                     && key is not "vnp_SecureHash" and not "vnp_SecureHashType"))
        {
            parameters[key] = query[key].ToString();
        }

        if (!parameters.TryGetValue("vnp_TmnCode", out var tmnCode)
            || !string.Equals(tmnCode, _tmnCode, StringComparison.Ordinal)) return false;

        byte[] expected;
        byte[] provided;
        try
        {
            expected = Convert.FromHexString(ComputeSignature(BuildQuery(parameters)));
            provided = Convert.FromHexString(providedSignature);
        }
        catch (FormatException)
        {
            return false;
        }

        return expected.Length == provided.Length
            && CryptographicOperations.FixedTimeEquals(expected, provided);
    }

    public static string BuildQuery(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        return string.Join("&", parameters.Select(parameter =>
            $"{WebUtility.UrlEncode(parameter.Key)}={WebUtility.UrlEncode(parameter.Value)}"));
    }

    private string ComputeSignature(string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_hashSecret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    private static DateTime GetVietnamTime()
    {
        var timeZoneId = OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh";
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }
}