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
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SystemController> _logger;

        public SystemController(
            WorkFinderContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IWebHostEnvironment environment,
            ILogger<SystemController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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

                await DataSeeder.SeedData(_context, _userManager, _roleManager, true);

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

                await DataSeeder.SeedData(_context, _userManager, _roleManager, false);

                TempData["SuccessMessage"] = "Dữ liệu mới đã được thêm vào database thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding fresh data");
                TempData["ErrorMessage"] = $"Lỗi khi seed dữ liệu mới: {ex.Message}";
            }

            return RedirectToAction(nameof(DatabaseManagement));
        }

        [HttpPost]
        public async Task<IActionResult> ResetAdminAccount()
        {
            try
            {
                string adminEmail = "admin@pth.com";
                string newPassword = "Admin@123456";

                _logger.LogWarning("Admin account reset initiated by user: {User}", User.Identity.Name);

                // Tìm tài khoản admin
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    // Tạo mới nếu không tồn tại
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "System",
                        LastName = "Administrator"
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, newPassword);
                    if (!createResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = $"Không thể tạo tài khoản admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}";
                        return RedirectToAction(nameof(DatabaseManagement));
                    }
                }
                else
                {
                    // Reset mật khẩu nếu tài khoản đã tồn tại
                    var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var resetResult = await _userManager.ResetPasswordAsync(adminUser, token, newPassword);
                    if (!resetResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = $"Không thể đặt lại mật khẩu admin: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}";
                        return RedirectToAction(nameof(DatabaseManagement));
                    }
                }

                // Đảm bảo role Admin tồn tại
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                {
                    var role = new IdentityRole<int>("Admin");
                    var roleResult = await _roleManager.CreateAsync(role);
                    if (!roleResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = $"Không thể tạo role Admin: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}";
                        return RedirectToAction(nameof(DatabaseManagement));
                    }
                }

                // Gán role Admin cho tài khoản admin
                if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    var addToRoleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!addToRoleResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = $"Không thể gán quyền Admin: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}";
                        return RedirectToAction(nameof(DatabaseManagement));
                    }
                }

                TempData["SuccessMessage"] = $"Tài khoản admin ({adminEmail}) đã được reset thành công với mật khẩu: {newPassword}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting admin account");
                TempData["ErrorMessage"] = $"Lỗi khi reset tài khoản admin: {ex.Message}";
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