using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;

namespace WorkFinder.Web.Areas.Auth.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {

            return null;
        }


        // Thử cách thông thường đầu tiên
        var appUser = await _userManager.GetUserAsync(user);

        // Nếu không tìm thấy, thử lấy user bằng ID từ claims
        if (appUser == null)
        {


            // Lấy userID từ claim
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                Console.WriteLine($"GetCurrentUserAsync: Trying to find user by ID {userId}");
                appUser = await _userManager.FindByIdAsync(userId.ToString());

                if (appUser != null)
                {

                }
            }

            // Log all claims for debugging
            var claims = user.Claims.Select(c => $"{c.Type}={c.Value}");

        }
        else
        {

        }

        return appUser;
    }

    public async Task<ProfileViewModel> GetProfileAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            throw new InvalidOperationException("Không thể tìm thấy người dùng hiện tại. Vui lòng đăng nhập lại.");



        return new ProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            DateOfBirth = user.DateOfBirth,
            CurrentProfilePicture = user.ProfilePicture
        };
    }

    public async Task<(bool succeeded, IEnumerable<string> errors)> UpdateProfileAsync(ProfileViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return (false, new[] { "Không thể tìm thấy người dùng hiện tại." });

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.DateOfBirth = model.DateOfBirth;

        // Xử lý ảnh đại diện nếu có
        if (model.ProfilePicture != null)
        {
            var profilePicturePath = await SaveProfilePictureAsync(model.ProfilePicture);
            if (profilePicturePath != null)
            {
                user.ProfilePicture = profilePicturePath;
            }
        }

        // Cập nhật email nếu thay đổi
        if (user.Email != model.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                return (false, setEmailResult.Errors.Select(e => e.Description));
            }

            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
            if (!setUserNameResult.Succeeded)
            {
                return (false, setUserNameResult.Errors.Select(e => e.Description));
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        return updateResult.Succeeded
            ? (true, Array.Empty<string>())
            : (false, updateResult.Errors.Select(e => e.Description));
    }

    public async Task<(bool succeeded, IEnumerable<string> errors)> ChangePasswordAsync(ChangePasswordViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return (false, new[] { "Không thể tìm thấy người dùng hiện tại." });

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        return result.Succeeded
            ? (true, Array.Empty<string>())
            : (false, result.Errors.Select(e => e.Description));
    }

    public async Task<string?> SaveProfilePictureAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        try
        {
            // Đảm bảo thư mục tồn tại
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-pictures");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file duy nhất
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối để lưu vào database
            return "/uploads/profile-pictures/" + uniqueFileName;
        }
        catch (Exception)
        {
            return null;
        }
    }
}