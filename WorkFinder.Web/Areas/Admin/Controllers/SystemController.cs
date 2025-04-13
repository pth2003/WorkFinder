using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SystemController : Controller
    {
        private readonly WorkFinderContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SystemController> _logger;

        public SystemController(
            WorkFinderContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<SystemController> logger)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DatabaseManagement()
        {
            var model = new DatabaseManagementViewModel
            {
                UserCount = _context.Users.Count(),
                CompanyCount = _context.Companies.Count(),
                JobCount = _context.Jobs.Count(),
                ApplicationCount = _context.JobApplications.Count(),
                CategoriesCount = _context.Categories.Count(),
                SqlFileExists = System.IO.File.Exists(Path.Combine(_environment.ContentRootPath, "WorkFinder.sql"))
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetDatabase(string confirmReset)
        {
            if (confirmReset != "CONFIRM_RESET")
            {
                TempData["ErrorMessage"] = "Xác nhận không chính xác. Database không được reset.";
                return RedirectToAction(nameof(DatabaseManagement));
            }

            try
            {
                _logger.LogWarning("Database reset initiated by admin user: {User}", User.Identity.Name);

                await DataSeeder.SeedData(_context, _userManager, true);

                TempData["SuccessMessage"] = "Database đã được reset và seed lại thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting database");
                TempData["ErrorMessage"] = $"Lỗi khi reset database: {ex.Message}";
            }

            return RedirectToAction(nameof(DatabaseManagement));
        }

        [HttpPost]
        public async Task<IActionResult> SeedFreshData()
        {
            try
            {
                _logger.LogInformation("Fresh data seeding initiated by admin user: {User}", User.Identity.Name);

                await DataSeeder.SeedData(_context, _userManager, false);

                TempData["SuccessMessage"] = "Dữ liệu mới đã được thêm vào database thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding fresh data");
                TempData["ErrorMessage"] = $"Lỗi khi seed dữ liệu mới: {ex.Message}";
            }

            return RedirectToAction(nameof(DatabaseManagement));
        }
    }

    public class DatabaseManagementViewModel
    {
        public int UserCount { get; set; }
        public int CompanyCount { get; set; }
        public int JobCount { get; set; }
        public int ApplicationCount { get; set; }
        public int CategoriesCount { get; set; }
        public bool SqlFileExists { get; set; }
    }
}