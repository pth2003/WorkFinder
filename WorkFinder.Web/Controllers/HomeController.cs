using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;

namespace WorkFinder.Web.Controllers;

public class HomeController : Controller
{
    private readonly WorkFinderContext _context;

    public HomeController(WorkFinderContext context)
    {
        _context = context;

    }

    public async Task<IActionResult> Index()
    {

        return View();
    }
}