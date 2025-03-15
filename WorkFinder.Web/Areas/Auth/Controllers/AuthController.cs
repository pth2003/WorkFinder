using Microsoft.AspNetCore.Mvc;

namespace WorkFinder.Web.Areas.Auth.Controllers;

public class AuthController : Controller
{
    [Area("Auth")]
    public IActionResult Login()
    {
        return View();
    }
}