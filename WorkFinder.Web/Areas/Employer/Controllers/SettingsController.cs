using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;
using WorkFinder.Web.Areas.Employer.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            ILogger<SettingsController> logger)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                return RedirectToAction("Create", "Company", new { area = "Employer" });
            }

            var model = new SettingsViewModel
            {
                UserId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                CompanyId = company.Id,
                CompanyName = company.Name,
                IsCompanyVerified = company.IsVerified,
                ProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(SettingsViewModel model)
        {
            // Skip validation for password fields when updating profile
            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                // Retrieve information from database to refill the form in case of errors
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var company = await _companyRepository.GetByOwnerIdAsync(currentUser.Id);
                    if (company != null)
                    {
                        model.CompanyId = company.Id;
                        model.CompanyName = company.Name;
                        model.IsCompanyVerified = company.IsVerified;
                    }
                    model.ProfilePicture = currentUser.ProfilePicture; // Preserve profile picture
                }
                return View("Index", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                // Save current ProfilePicture before update
                string currentProfilePicture = user.ProfilePicture;

                // Update only the basic user properties from ApplicationUser
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;

                // Convert DateTime to UTC if it has a value
                if (model.DateOfBirth.HasValue)
                {
                    // If Kind=Unspecified, assume it's local time and convert to UTC
                    if (model.DateOfBirth.Value.Kind == DateTimeKind.Unspecified)
                    {
                        DateTime localTime = DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Local);
                        user.DateOfBirth = localTime.ToUniversalTime();
                    }
                    else if (model.DateOfBirth.Value.Kind == DateTimeKind.Local)
                    {
                        // Convert from local time to UTC
                        user.DateOfBirth = model.DateOfBirth.Value.ToUniversalTime();
                    }
                    else
                    {
                        // Already UTC
                        user.DateOfBirth = model.DateOfBirth;
                    }
                }
                else
                {
                    user.DateOfBirth = null;
                }

                // Make sure not to overwrite ProfilePicture with an empty value
                if (string.IsNullOrEmpty(model.ProfilePicture))
                {
                    user.ProfilePicture = currentProfilePicture;
                }
                else
                {
                    user.ProfilePicture = model.ProfilePicture;
                }

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("Index", model);
                }

                TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SettingsViewModel model)
        {
            // Chỉ đánh giá các trường liên quan đến mật khẩu
            var passwordValidationContext = new ValidationContext(model, serviceProvider: null, items: null);
            var passwordValidationResults = new List<ValidationResult>();
            var passwordProperties = new[] { "CurrentPassword", "NewPassword", "ConfirmPassword" };

            if (!Validator.TryValidateProperty(model.CurrentPassword,
                new ValidationContext(model, null, null) { MemberName = "CurrentPassword" }, passwordValidationResults) ||
                !Validator.TryValidateProperty(model.NewPassword,
                new ValidationContext(model, null, null) { MemberName = "NewPassword" }, passwordValidationResults) ||
                !Validator.TryValidateProperty(model.ConfirmPassword,
                new ValidationContext(model, null, null) { MemberName = "ConfirmPassword" }, passwordValidationResults))
            {
                foreach (var validationResult in passwordValidationResults)
                {
                    ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                }
            }

            if (string.IsNullOrEmpty(model.CurrentPassword) ||
                string.IsNullOrEmpty(model.NewPassword) ||
                string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin mật khẩu");
                TempData["ActiveTab"] = "password";

                // Fill user data
                await FillUserData(model);
                return View("Index", model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp");
                TempData["ActiveTab"] = "password";

                // Fill user data
                await FillUserData(model);
                return View("Index", model);
            }

            if (model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu phải có ít nhất 6 ký tự");
                TempData["ActiveTab"] = "password";

                // Fill user data
                await FillUserData(model);
                return View("Index", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["ActiveTab"] = "password";

                    // Fill user data
                    await FillUserData(model);
                    return View("Index", model);
                }

                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
                TempData["ActiveTab"] = "password";

                // Fill user data
                await FillUserData(model);
                return View("Index", model);
            }
        }

        // Helper để điền dữ liệu người dùng
        private async Task FillUserData(SettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
                if (company != null)
                {
                    model.CompanyId = company.Id;
                    model.CompanyName = company.Name;
                    model.IsCompanyVerified = company.IsVerified;
                }

                // Điền thông tin cơ bản nếu chưa có
                if (string.IsNullOrEmpty(model.FirstName))
                    model.FirstName = user.FirstName;
                if (string.IsNullOrEmpty(model.LastName))
                    model.LastName = user.LastName;
                if (string.IsNullOrEmpty(model.Email))
                    model.Email = user.Email;
                if (string.IsNullOrEmpty(model.PhoneNumber))
                    model.PhoneNumber = user.PhoneNumber;
                if (model.DateOfBirth == null)
                    model.DateOfBirth = user.DateOfBirth;

                // Đảm bảo giữ nguyên ảnh đại diện
                model.ProfilePicture = user.ProfilePicture;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(string userId, IFormFile avatarFile)
        {
            try
            {
                if (avatarFile == null || avatarFile.Length == 0)
                {
                    TempData["AvatarError"] = "Please select a file to upload.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate file size (max 5MB)
                if (avatarFile.Length > 5 * 1024 * 1024)
                {
                    TempData["AvatarError"] = "File size must be less than 5MB.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate file type
                string fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) ||
                    !(fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".gif"))
                {
                    TempData["AvatarError"] = "Only JPG, PNG, and GIF files are allowed.";
                    return RedirectToAction(nameof(Index));
                }

                // Get current user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                // Update user profile picture path in database
                var avatarUrl = $"/uploads/avatars/{uniqueFileName}";
                user.ProfilePicture = avatarUrl;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["AvatarError"] = "Failed to update profile picture.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = "Profile picture updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error uploading avatar");
                TempData["AvatarError"] = "An error occurred while uploading the profile picture.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteAccount")]
        public async Task<IActionResult> DeleteAccountConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company != null)
            {
                await _companyRepository.DeleteAsync(company.Id);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}