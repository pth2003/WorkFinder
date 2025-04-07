using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace WorkFinder.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IResumeRepository _resumeRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAccountService accountService,
        IResumeRepository resumeRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _resumeRepository = resumeRepository ?? throw new ArgumentNullException(nameof(resumeRepository));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        _logger.LogInformation("Profile page accessed");
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (succeeded, errors) = await _accountService.UpdateProfileAsync(model);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công.";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (succeeded, errors) = await _accountService.ChangePasswordAsync(model);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        try
        {
            _logger.LogInformation("Settings page accessed");
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth", new { area = "Auth" });

            var profileModel = await _accountService.GetProfileAsync();
            var resume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

            var model = new AccountSettingsViewModel
            {
                Profile = profileModel
            };

            if (resume != null)
            {
                model.Resume = resume;
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing settings page");
            TempData["ErrorMessage"] = $"Error: {ex.Message}";

            // Create a basic model to avoid errors
            var model = new AccountSettingsViewModel
            {
                Profile = new ProfileViewModel()
            };

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBasicInfo(ProfileViewModel model)
    {
        try
        {
            _logger.LogInformation("Updating basic info");
            // Get current user directly
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth", new { area = "Auth" });

            // Update only the basic user properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            // Convert DateTime to UTC if it has a value
            if (model.DateOfBirth.HasValue)
            {
                // Create a new UTC DateTime explicitly
                // First get just the date part without time component
                var dateOnly = model.DateOfBirth.Value.Date;

                // Then create a new DateTime with explicit UTC kind
                user.DateOfBirth = new DateTime(
                    dateOnly.Year,
                    dateOnly.Month,
                    dateOnly.Day,
                    0, 0, 0,
                    DateTimeKind.Utc);
            }
            else
            {
                user.DateOfBirth = null;
            }

            // Use UserManager to save changes directly
            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Thông tin cơ bản đã được cập nhật thành công.";

                // Get current data to display after successful update
                var updatedProfile = await _accountService.GetProfileAsync();
                var userResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                var settingsViewModel = new AccountSettingsViewModel
                {
                    Profile = updatedProfile
                };

                if (userResume != null)
                {
                    settingsViewModel.Resume = userResume;
                }

                return View("Settings", settingsViewModel);
            }

            // If update fails, show errors
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Get current data to display on error
            var profileModel = await _accountService.GetProfileAsync();
            var resume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

            var viewModel = new AccountSettingsViewModel
            {
                Profile = profileModel
            };

            if (resume != null)
            {
                viewModel.Resume = resume;
            }

            return View("Settings", viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Settings));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
    {
        try
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                _logger.LogWarning("Profile picture upload attempted with no file");
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh để tải lên.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();
                var currentResume = await _resumeRepository.GetResumeByUserIdAsync(
                    (await _accountService.GetCurrentUserAsync())?.Id ?? 0);

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                if (currentResume != null)
                {
                    currentViewModel.Resume = currentResume;
                }

                return View("Settings", currentViewModel);
            }

            // Check if file is an image
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning($"Invalid file format attempted: {extension}");
                TempData["ErrorMessage"] = "Chỉ chấp nhận các định dạng ảnh: JPG, JPEG, PNG, GIF.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();
                var currentResume = await _resumeRepository.GetResumeByUserIdAsync(
                    (await _accountService.GetCurrentUserAsync())?.Id ?? 0);

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                if (currentResume != null)
                {
                    currentViewModel.Resume = currentResume;
                }

                return View("Settings", currentViewModel);
            }

            // Get current user
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Auth" });
            }

            // Save profile picture
            var profilePicturePath = await SaveProfilePictureAsync(profilePicture);

            if (profilePicturePath == null)
            {
                TempData["ErrorMessage"] = "Không thể tải lên ảnh đại diện. Vui lòng thử lại.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();
                var currentResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                if (currentResume != null)
                {
                    currentViewModel.Resume = currentResume;
                }

                return View("Settings", currentViewModel);
            }

            // Update user profile picture directly
            user.ProfilePicture = profilePicturePath;
            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Ảnh đại diện đã được cập nhật thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            }

            // Get current data to display
            var updatedProfile = await _accountService.GetProfileAsync();
            var userResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

            var viewModel = new AccountSettingsViewModel
            {
                Profile = updatedProfile
            };

            if (userResume != null)
            {
                viewModel.Resume = userResume;
            }

            return View("Settings", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile picture");
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";

            try
            {
                // Get current data to display on error
                var errorProfile = await _accountService.GetProfileAsync();
                var errorResume = await _resumeRepository.GetResumeByUserIdAsync(
                    (await _accountService.GetCurrentUserAsync())?.Id ?? 0);

                var errorViewModel = new AccountSettingsViewModel
                {
                    Profile = errorProfile
                };

                if (errorResume != null)
                {
                    errorViewModel.Resume = errorResume;
                }

                return View("Settings", errorViewModel);
            }
            catch
            {
                // Fallback if we can't load the profile
                return View("Settings", new AccountSettingsViewModel
                {
                    Profile = new ProfileViewModel()
                });
            }
        }
    }

    private async Task<string?> SaveProfilePictureAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        try
        {
            // Ensure directory exists
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile-pictures");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Create unique filename
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path to store in database
            return "/uploads/profile-pictures/" + uniqueFileName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadResume(IFormFile file)
    {
        try
        {
            _logger.LogInformation("Resume upload initiated");
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth", new { area = "Auth" });

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Resume upload attempted with no file");
                TempData["ErrorMessage"] = "Please select a file.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();
                var currentResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                if (currentResume != null)
                {
                    currentViewModel.Resume = currentResume;
                }

                return View("Settings", currentViewModel);
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                TempData["ErrorMessage"] = "Only PDF files are supported.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();
                var currentResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                if (currentResume != null)
                {
                    currentViewModel.Resume = currentResume;
                }

                return View("Settings", currentViewModel);
            }

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var resumeUrl = $"/uploads/resumes/{fileName}";

            // Check if user already has a resume
            var existingResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

            if (existingResume != null)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingResume.FileUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingResume.FileUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Update existing resume
                existingResume.FileUrl = resumeUrl;
                await _resumeRepository.UpdateAsync(existingResume);
                await _resumeRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "Resume updated successfully.";
            }
            else
            {
                // Create new resume
                var resume = new Resume
                {
                    UserId = user.Id,
                    FileUrl = resumeUrl,
                    Summary = string.Empty,
                    Skills = string.Empty,
                    Education = string.Empty,
                    Experience = string.Empty,
                    Certifications = string.Empty,
                    Languages = string.Empty
                };

                await _resumeRepository.AddResumeAsync(resume);
                TempData["SuccessMessage"] = "Resume uploaded successfully.";
            }

            // Get current data to display
            var updatedProfile = await _accountService.GetProfileAsync();
            var updatedResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

            var viewModel = new AccountSettingsViewModel
            {
                Profile = updatedProfile
            };

            if (updatedResume != null)
            {
                viewModel.Resume = updatedResume;
            }

            return View("Settings", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading resume");
            TempData["ErrorMessage"] = $"Error: {ex.Message}";

            try
            {
                // Get current data to display on error
                var errorProfile = await _accountService.GetProfileAsync();
                var errorResume = await _resumeRepository.GetResumeByUserIdAsync(
                    (await _accountService.GetCurrentUserAsync())?.Id ?? 0);

                var errorViewModel = new AccountSettingsViewModel
                {
                    Profile = errorProfile
                };

                if (errorResume != null)
                {
                    errorViewModel.Resume = errorResume;
                }

                return View("Settings", errorViewModel);
            }
            catch
            {
                // Fallback if we can't load the profile
                return View("Settings", new AccountSettingsViewModel
                {
                    Profile = new ProfileViewModel()
                });
            }
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResume(int id)
    {
        try
        {
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth", new { area = "Auth" });

            var resume = await _resumeRepository.GetByIdAsync(id);
            if (resume == null)
            {
                TempData["ErrorMessage"] = "Resume not found.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                return View("Settings", currentViewModel);
            }

            if (resume.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this resume.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();
                var currentResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                if (currentResume != null)
                {
                    currentViewModel.Resume = currentResume;
                }

                return View("Settings", currentViewModel);
            }

            // Delete file if exists
            if (!string.IsNullOrEmpty(resume.FileUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resume.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _resumeRepository.DeleteAsync(id);
            await _resumeRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Resume deleted successfully.";

            // Get current data to display
            var updatedProfile = await _accountService.GetProfileAsync();

            var viewModel = new AccountSettingsViewModel
            {
                Profile = updatedProfile
            };

            return View("Settings", viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";

            try
            {
                // Get current data to display on error
                var errorProfile = await _accountService.GetProfileAsync();
                var errorResume = await _resumeRepository.GetResumeByUserIdAsync(
                    (await _accountService.GetCurrentUserAsync())?.Id ?? 0);

                var errorViewModel = new AccountSettingsViewModel
                {
                    Profile = errorProfile
                };

                if (errorResume != null)
                {
                    errorViewModel.Resume = errorResume;
                }

                return View("Settings", errorViewModel);
            }
            catch
            {
                // Fallback if we can't load the profile
                return View("Settings", new AccountSettingsViewModel
                {
                    Profile = new ProfileViewModel()
                });
            }
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateResumeDetails(int id, string Summary, string Skills, string Experience, string Education, string Certifications, string Languages)
    {
        try
        {
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth", new { area = "Auth" });

            var resume = await _resumeRepository.GetByIdAsync(id);
            if (resume == null)
            {
                TempData["ErrorMessage"] = "Resume not found.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                return View("Settings", currentViewModel);
            }

            if (resume.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "You don't have permission to update this resume.";

                // Get current data to display
                var currentProfile = await _accountService.GetProfileAsync();
                var currentResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                var currentViewModel = new AccountSettingsViewModel
                {
                    Profile = currentProfile
                };

                if (currentResume != null)
                {
                    currentViewModel.Resume = currentResume;
                }

                return View("Settings", currentViewModel);
            }

            // Update resume details
            resume.Summary = Summary ?? string.Empty;
            resume.Skills = Skills ?? string.Empty;
            resume.Experience = Experience ?? string.Empty;
            resume.Education = Education ?? string.Empty;
            resume.Certifications = Certifications ?? string.Empty;
            resume.Languages = Languages ?? string.Empty;

            await _resumeRepository.UpdateAsync(resume);
            await _resumeRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Resume details updated successfully.";

            // Get current data to display
            var updatedProfile = await _accountService.GetProfileAsync();
            var updatedResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

            var viewModel = new AccountSettingsViewModel
            {
                Profile = updatedProfile
            };

            if (updatedResume != null)
            {
                viewModel.Resume = updatedResume;
            }

            return View("Settings", viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";

            try
            {
                // Get current data to display on error
                var errorProfile = await _accountService.GetProfileAsync();
                var errorResume = await _resumeRepository.GetResumeByUserIdAsync(
                    (await _accountService.GetCurrentUserAsync())?.Id ?? 0);

                var errorViewModel = new AccountSettingsViewModel
                {
                    Profile = errorProfile
                };

                if (errorResume != null)
                {
                    errorViewModel.Resume = errorResume;
                }

                return View("Settings", errorViewModel);
            }
            catch
            {
                // Fallback if we can't load the profile
                return View("Settings", new AccountSettingsViewModel
                {
                    Profile = new ProfileViewModel()
                });
            }
        }
    }
}