using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;

namespace WorkFinder.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var model = await _accountService.GetProfileAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
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
}