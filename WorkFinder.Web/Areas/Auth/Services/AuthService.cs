using Microsoft.AspNetCore.Identity;
using WorkFinder.Web.Areas.Auth.Models;
using WorkFinder.Web.Models;


namespace WorkFinder.Web.Areas.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(bool succeeded, string[] errors)> RegisterAsync(RegisterViewModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }
    
    public async Task<(bool succeeded, string[] errors)> LoginAsync(LoginViewModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        return (result.Succeeded, result.Succeeded ? Array.Empty<string>() : new[] { "Invalid login attempt." });
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
    }
}