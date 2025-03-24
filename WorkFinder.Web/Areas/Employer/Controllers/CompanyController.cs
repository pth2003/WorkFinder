using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkFinder.Web.Areas.Employer.Models;
using WorkFinder.Web.Areas.Employer.Services;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = UserRoles.Employer)]
    public class CompanyController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanySetupService _setupService;
        private readonly ICategoryRepository _categoryRepository;

        public CompanyController(
            ICompanyRepository companyRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ICompanySetupService setupService,
            ICategoryRepository categoryRepository)
        {
            _companyRepository = companyRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _setupService = setupService;
            _categoryRepository = categoryRepository;
        }

        #region Setup Step 1: Basic Info

        // GET: Step 1
        public async Task<IActionResult> SetupBasic()
        {
            // Kiểm tra xem có thông tin từ session không
            var model = _setupService.GetBasicInfo();

            // Nếu không có thông tin trong session, thử lấy từ database
            if (string.IsNullOrEmpty(model.Name) && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                // Nếu công ty đã tồn tại, điền thông tin vào model
                if (company != null)
                {
                    model.Name = company.Name;
                    model.Description = company.Description;
                    model.LogoPath = company.Logo;
                    model.BannerPath = company.Banner;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetupBasic(CompanySetupBasicViewModel model)
        {
            // Xóa validation errors cho LogoPath và BannerPath nếu có
            ModelState.Remove("LogoPath");
            ModelState.Remove("BannerPath");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                // Kiểm tra session trước khi lưu
                if (HttpContext.Session == null)
                {
                    Console.WriteLine("HttpContext.Session is null!");
                    ModelState.AddModelError(string.Empty, "Session is not available");
                    return View(model);
                }

                Console.WriteLine($"Attempting to save: Name={model.Name}, Description={model.Description?.Substring(0, Math.Min(model.Description?.Length ?? 0, 20))}...");

                // Lưu thông tin vào session
                await _setupService.SaveBasicInfoAsync(model);

                // Kiểm tra xem đã lưu thành công chưa bằng cách đọc lại từ session
                var savedModel = _setupService.GetBasicInfo();
                if (savedModel == null || string.IsNullOrEmpty(savedModel.Name))
                {
                    Console.WriteLine("Failed to save data to session!");
                    ModelState.AddModelError(string.Empty, "Failed to save data to session");
                    return View(model);
                }

                Console.WriteLine($"Successfully saved to session: Name={savedModel.Name}");

                // Thêm thông báo thành công
                TempData["SuccessMessage"] = "Basic information saved successfully!";

                // Chuyển đến bước tiếp theo
                return RedirectToAction("SetupOrganization");
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Error in SetupBasic: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Thêm thông báo lỗi
                ModelState.AddModelError(string.Empty, $"Error saving information: {ex.Message}");
                return View(model);
            }
        }

        #endregion

        #region Setup Step 2: Organization Info

        public async Task<IActionResult> SetupOrganization()
        {
            // Kiểm tra xem đã hoàn thành bước 1 chưa
            if (!_setupService.HasCompletedBasicInfo())
            {
                return RedirectToAction("SetupBasic");
            }

            // Lấy thông tin từ session
            var model = _setupService.GetOrganizationInfo();

            // Nếu không có thông tin trong session, thử lấy từ database
            if (model.Industry == null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                // Nếu công ty đã tồn tại, điền thông tin vào model
                if (company != null)
                {
                    model.CategoryId = company.CategoryId;
                    model.Industry = company.Industry;
                    model.EmployeeCount = company.EmployeeCount;
                    model.FoundedDate = company.FoundedDate;
                    model.Website = company.Website;
                    model.Vision = company.Vision;
                }
            }

            // Lấy danh sách category để hiển thị dropdown
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetupOrganization(CompanySetupOrganizationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                return View(model);
            }

            // Lưu thông tin vào session
            await _setupService.SaveOrganizationInfoAsync(model);

            // Chuyển đến bước tiếp theo
            return RedirectToAction("SetupSocial");
        }

        #endregion

        #region Setup Step 3: Social Media

        public async Task<IActionResult> SetupSocial()
        {
            // Kiểm tra xem đã hoàn thành bước 2 chưa
            if (!_setupService.HasCompletedOrganizationInfo())
            {
                return RedirectToAction("SetupOrganization");
            }

            // Lấy thông tin từ session
            var model = _setupService.GetSocialInfo();

            // Nếu không có social links trong session, thử lấy từ database
            if (model.SocialLinks.Count == 0 && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                // Nếu công ty đã tồn tại và có social links
                if (company != null && company.SocialLinks?.Count > 0)
                {
                    model.SocialLinks = company.SocialLinks.Select(s => new SocialLinkViewModel
                    {
                        Id = s.Id,
                        Platform = s.Platform,
                        Url = s.Url
                    }).ToList();
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetupSocial(CompanySetupSocialViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Lưu thông tin vào session
            await _setupService.SaveSocialInfoAsync(model);

            // Chuyển đến bước tiếp theo
            return RedirectToAction("SetupContact");
        }

        #endregion

        #region Setup Step 4: Contact Info

        public async Task<IActionResult> SetupContact()
        {
            // Kiểm tra xem đã hoàn thành bước 3 chưa
            if (!_setupService.HasCompletedSocialInfo())
            {
                return RedirectToAction("SetupSocial");
            }

            // Lấy thông tin từ session
            var model = _setupService.GetContactInfo();

            // Nếu không có thông tin trong session, thử lấy từ database
            if (string.IsNullOrEmpty(model.Location) && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                // Nếu công ty đã tồn tại, điền thông tin vào model
                if (company != null)
                {
                    model.Location = company.Location;
                    model.Phone = company.Phone;
                    model.Email = company.Email;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetupContact(CompanySetupContactViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Hoàn tất quá trình setup và lưu dữ liệu vào database
                var company = await _setupService.CompleteSetupAsync(model);

                // Hiển thị thông báo thành công
                TempData["SuccessMessage"] = "Company setup completed successfully!";

                // Chuyển đến trang dashboard của employer
                return RedirectToAction("Index", "Dashboard", new { area = "Employer" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error saving company information: " + ex.Message);
                return View(model);
            }
        }

        #endregion

        // Helper method to save file
        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}