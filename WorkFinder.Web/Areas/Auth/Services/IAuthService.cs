using Microsoft.AspNetCore.Identity;
using WorkFinder.Web.Areas.Auth.Models;
using WorkFinder.Web.Models;


namespace WorkFinder.Web.Areas.Auth.Services;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterViewModel model);
    Task<(bool succeeded, string[] errors)> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
    Task<ApplicationUser?> GetCurrentUserAsync();
}