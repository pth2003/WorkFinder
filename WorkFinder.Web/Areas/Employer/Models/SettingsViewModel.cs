using System.ComponentModel.DataAnnotations;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class SettingsViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ")]
        [Display(Name = "Họ")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Tên")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; }

        // Company Info
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public bool IsCompanyVerified { get; set; } = true;

        // Password Change
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Flags for tab display
        public bool ShowProfileTab { get; set; } = true;
        public bool ShowPasswordTab { get; set; } = false;
        public bool ShowCompanyTab { get; set; } = false;
        public bool ShowDataTab { get; set; } = false;
    }
}