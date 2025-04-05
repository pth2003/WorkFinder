using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WorkFinder.Web.Models.ViewModels;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Tên là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
    [Display(Name = "Tên")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ là bắt buộc")]
    [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
    [Display(Name = "Họ")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Ảnh đại diện hiện tại")]
    public string? CurrentProfilePicture { get; set; }

    [Display(Name = "Tải lên ảnh đại diện mới")]
    public IFormFile? ProfilePicture { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu mới")]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}