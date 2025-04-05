using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace WorkFinder.Web.Areas.Auth.Services;

public interface IAccountService
{
    Task<ApplicationUser?> GetCurrentUserAsync();
    Task<ProfileViewModel> GetProfileAsync();
    Task<(bool succeeded, IEnumerable<string> errors)> UpdateProfileAsync(ProfileViewModel model);
    Task<(bool succeeded, IEnumerable<string> errors)> ChangePasswordAsync(ChangePasswordViewModel model);
    Task<string?> SaveProfilePictureAsync(IFormFile file);
}