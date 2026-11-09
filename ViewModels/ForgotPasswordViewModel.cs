using System.ComponentModel.DataAnnotations;

namespace CoffeeShopMVC.ViewModels;

public class ForgotPasswordViewModel
{
    [Required, MaxLength(20)]
    [RegularExpression(@"^[0-9+()\-\s]{9,20}$", ErrorMessage = "Vui lòng nhập số điện thoại hợp lệ.")]
    public string Phone { get; set; } = string.Empty;
}