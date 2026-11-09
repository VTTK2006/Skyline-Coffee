using System.ComponentModel.DataAnnotations;

namespace CoffeeShopMVC.ViewModels;

public class VerifyPasswordResetViewModel
{
    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP gồm 6 chữ số.")]
    public string Otp { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}