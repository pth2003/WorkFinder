using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;
using WorkFinder.Web.Areas.Employer.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            ILogger<SettingsController> logger)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                return RedirectToAction("Create", "Company", new { area = "Employer" });
            }

            var model = new SettingsViewModel
            {
                UserId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CompanyId = company.Id,
                CompanyName = company.Name,
                IsCompanyVerified = company.IsVerified,
                ProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "Thông tin cá nhân đã được cập nhật thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCompanyVisibility(int companyId, bool isVisible)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null || company.OwnerId != user.Id)
            {
                return NotFound();
            }

            company.IsVerified = isVisible;
            await _companyRepository.UpdateAsync(company);

            TempData["SuccessMessage"] = isVisible
                ? "Thông tin công ty của bạn đã được hiển thị công khai."
                : "Thông tin công ty của bạn đã được ẩn.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(model.CurrentPassword) ||
                string.IsNullOrEmpty(model.NewPassword) ||
                string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin mật khẩu");
                return View("Index", model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp");
                return View("Index", model);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportCandidates()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                return NotFound();
            }

            var (jobs, _) = await _jobRepository.GetAllJobsByCompanyIdAsync(company.Id, 1, 1000);
            var applications = jobs.SelectMany(j => j.Applications).ToList();

            if (!applications.Any())
            {
                TempData["WarningMessage"] = "Không có ứng viên nào để xuất.";
                return RedirectToAction(nameof(Index));
            }

            // TODO: Implement actual CSV/Excel export logic here
            // For now just return a placeholder text file
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes("Export data placeholder");
            return File(fileBytes, "text/plain", $"candidates_{DateTime.Now:yyyyMMdd}.txt");
        }

        [HttpGet]
        public async Task<IActionResult> BackupData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                return NotFound();
            }

            // TODO: Implement actual backup logic
            // For now just return a placeholder file
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes("Backup data placeholder");
            return File(fileBytes, "application/octet-stream", $"workfinder_backup_{DateTime.Now:yyyyMMdd}.bak");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteAccount")]
        public async Task<IActionResult> DeleteAccountConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company != null)
            {
                await _companyRepository.DeleteAsync(company.Id);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}