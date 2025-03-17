using Microsoft.AspNetCore.Mvc;
using WorkFinder.Web.Areas.Auth.Models;
using WorkFinder.Web.Areas.Auth.Services;

namespace WorkFinder.Web.Areas.Auth.Controllers;
[Area("Auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    // register page
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (succeeded, errors) = await _authService.RegisterAsync(model);
        if (succeeded)
        {
            // Automatically log in the user after registration
            var loginResult = await _authService.LoginAsync(new LoginViewModel
            {
                Email = model.Email,
                Password = model.Password
            });

            if (loginResult.succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }
        }

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }


    // login page
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var (succeeded, errors) = await _authService.LoginAsync(model);
        if (succeeded)
        {

            return LocalRedirect(returnUrl ?? "/");
        }
        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
        return View(model);

    }

    // logout action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home", new { area = "" });
    }
}