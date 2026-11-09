using System.Globalization;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeeShopMVC.Data;
using CoffeeShopMVC.Models;
using CoffeeShopMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Controllers;

[AllowAnonymous]
public class VnpayController : Controller
{
    private readonly CoffeeDbContext _db;
    private readonly VnpayPaymentService _vnpay;

    public VnpayController(CoffeeDbContext db, VnpayPaymentService vnpay)
    {
        _db = db;
        _vnpay = vnpay;
    }

    [HttpGet]
    public async Task<IActionResult> Return()
    {
        if (!_vnpay.TryValidateResponse(Request.Query, out var parameters)
            || !TryGetOrderId(parameters, out var orderId)
            || !parameters.TryGetValue("vnp_Amount", out var amountValue)
            || !long.TryParse(amountValue, NumberStyles.None, CultureInfo.InvariantCulture, out var paidAmount))
        {
            TempData["Error"] = "Không thể xác minh phản hồi thanh toán từ VNPay.";
            return RedirectToAction("Index", "Home");
        }

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null || order.PaymentMethod != "VNPay"
            || paidAmount != checked((long)(order.TotalPrice * 100m)))
        {
            TempData["Error"] = "Thông tin giao dịch không khớp với đơn hàng.";
            return RedirectToAction("Index", "Home");
        }

        var success = parameters.GetValueOrDefault("vnp_ResponseCode") == "00"
            && parameters.GetValueOrDefault("vnp_TransactionStatus") == "00";
        if (success && !await ConfirmSuccessfulPaymentAsync(orderId))
        {
            TempData["Error"] = "Không thể cập nhật đơn hàng sau khi xác minh thanh toán.";
            return RedirectToAction("Index", "Home");
        }

        TempData["VnpayResult"] = success ? "success" : "failed";
        return RedirectToAction("Success", "Order", new { id = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> Ipn()
    {
        if (!_vnpay.TryValidateResponse(Request.Query, out var parameters))
            return IpnResult("97", "Invalid signature");

        if (!TryGetOrderId(parameters, out var orderId))
            return IpnResult("01", "Order not found");

        var order = await _db.Orders.Include(item => item.OrderDetails)
            .FirstOrDefaultAsync(item => item.Id == orderId);
        if (order == null) return IpnResult("01", "Order not found");

        if (order.PaymentMethod != "VNPay"
            || !parameters.TryGetValue("vnp_Amount", out var amountValue)
            || !long.TryParse(amountValue, NumberStyles.None, CultureInfo.InvariantCulture, out var paidAmount)
            || paidAmount != checked((long)(order.TotalPrice * 100m)))
            return IpnResult("04", "Invalid amount");

        var paymentSucceeded = parameters.GetValueOrDefault("vnp_ResponseCode") == "00"
            && parameters.GetValueOrDefault("vnp_TransactionStatus") == "00";
        if (paymentSucceeded)
        {
            return await ConfirmSuccessfulPaymentAsync(orderId)
                ? IpnResult("00", "Confirm Success")
                : IpnResult("02", "Order already confirmed");
        }

        if (order.Status != "Chờ thanh toán")
            return IpnResult("02", "Order already confirmed");

        order.Status = "Đã hủy";
        order.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return IpnResult("00", "Confirm Success");
    }

    private async Task<bool> ConfirmSuccessfulPaymentAsync(int orderId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order = await _db.Orders.Include(item => item.OrderDetails)
            .FirstOrDefaultAsync(item => item.Id == orderId);
        if (order == null) return false;
        await _db.Entry(order).ReloadAsync();
        if (order.Status == "Chờ xử lý") return true;
        if (order.Status != "Chờ thanh toán") return false;

        order.Status = "Chờ xử lý";
        order.UpdatedAt = DateTime.Now;
        await RemovePaidItemsFromCartAsync(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private async Task RemovePaidItemsFromCartAsync(Order order)
    {
        foreach (var detail in order.OrderDetails)
        {
            var cartItem = await _db.CartItems.FirstOrDefaultAsync(item =>
                item.UserId == order.UserId
                && item.ProductId == detail.ProductId
                && item.Size == detail.Size
                && item.Topping == detail.Topping);
            if (cartItem == null) continue;

            cartItem.Quantity -= detail.Quantity;
            if (cartItem.Quantity <= 0)
                _db.CartItems.Remove(cartItem);
        }
    }

    private static bool TryGetOrderId(IReadOnlyDictionary<string, string> parameters, out int orderId)
    {
        orderId = 0;
        return parameters.TryGetValue("vnp_TxnRef", out var transactionReference)
            && int.TryParse(transactionReference, NumberStyles.None, CultureInfo.InvariantCulture, out orderId);
    }

    private ContentResult IpnResult(string code, string message)
    {
        var response = JsonSerializer.Serialize(new VnpayIpnResponse(code, message));
        return Content(response, "application/json");
    }

    private sealed record VnpayIpnResponse(
        [property: JsonPropertyName("RspCode")] string RspCode,
        [property: JsonPropertyName("Message")] string Message);
}