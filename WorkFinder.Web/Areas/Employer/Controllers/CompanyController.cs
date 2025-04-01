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

        #region Company Profile

        // GET: Employer/Company/Profile
        public async Task<IActionResult> Profile()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Auth" });
            }

            // Check if the user has a company
            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

            // If no company exists, redirect to the setup flow
            if (company == null)
            {
                return RedirectToAction("SetupBasic");
            }

            // Create a view model with all company information
            var basicInfo = new CompanySetupBasicViewModel
            {
                Name = company.Name,
                Description = company.Description,
                LogoPath = company.Logo,
                BannerPath = company.Banner
            };

            var organizationInfo = new CompanySetupOrganizationViewModel
            {
                CategoryId = company.CategoryId,
                Industry = company.Industry,
                EmployeeCount = company.EmployeeCount,
                FoundedDate = company.FoundedDate,
                Website = company.Website,
                Vision = company.Vision
            };

            var socialInfo = new CompanySetupSocialViewModel
            {
                SocialLinks = company.SocialLinks?.Select(sl => new SocialLinkViewModel
                {
                    Id = sl.Id,
                    Platform = sl.Platform,
                    Url = sl.Url
                }).ToList() ?? new List<SocialLinkViewModel>()
            };

            var contactInfo = new CompanySetupContactViewModel
            {
                Location = company.Location,
                Phone = company.Phone,
                Email = company.Email
            };

            // Load categories for dropdown
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            // Create a combined view model for the tabbed interface
            var viewModel = new CompanyProfileViewModel
            {
                CompanyId = company.Id,
                BasicInfo = basicInfo,
                OrganizationInfo = organizationInfo,
                SocialInfo = socialInfo,
                ContactInfo = contactInfo,
                ActiveTab = "basic" // Default active tab
            };

            return View(viewModel);
        }

        // POST: Employer/Company/UpdateBasicInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBasicInfo(CompanySetupBasicViewModel model)
        {
            // Loại bỏ validation cho các trường liên quan đến file upload
            ModelState.Remove("Logo");
            ModelState.Remove("Banner");
            ModelState.Remove("LogoPath");
            ModelState.Remove("BannerPath");

            // Return validation errors if any
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Validation failed", errors = errors });
                }
                return RedirectToAction("Profile", new { tab = "basic", error = "Invalid input data" });
            }

            try
            {
                // Get the current user's company
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                if (company == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Company not found" });
                    }
                    return RedirectToAction("SetupBasic");
                }

                // Kiểm tra các trường thay đổi và cập nhật chỉ khi có thay đổi
                bool hasChanges = false;

                // Update name if changed
                if (!string.IsNullOrEmpty(model.Name) && company.Name != model.Name)
                {
                    company.Name = model.Name;
                    hasChanges = true;
                }

                // Update description if changed
                if (!string.IsNullOrEmpty(model.Description) && company.Description != model.Description)
                {
                    company.Description = model.Description;
                    hasChanges = true;
                }

                // Process logo if provided
                string logoPath = company.Logo;
                if (model.Logo != null && model.Logo.Length > 0)
                {
                    logoPath = await SaveFileAsync(model.Logo, "companies/logos");
                    company.Logo = logoPath;
                    hasChanges = true;
                }

                // Process banner if provided
                string bannerPath = company.Banner;
                if (model.Banner != null && model.Banner.Length > 0)
                {
                    bannerPath = await SaveFileAsync(model.Banner, "companies/banners");
                    company.Banner = bannerPath;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    company.UpdatedAt = DateTime.UtcNow;

                    // Save changes
                    await _companyRepository.UpdateAsync(company);
                    int saveResult = await _companyRepository.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Company basic information updated successfully!",
                            logoPath = logoPath,
                            bannerPath = bannerPath,
                            company = new
                            {
                                id = company.Id,
                                name = company.Name,
                                description = company.Description,
                                updatedAt = company.UpdatedAt
                            },
                            saveResult = saveResult,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["SuccessMessage"] = "Company basic information updated successfully!";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "No changes detected!",
                            logoPath = logoPath,
                            bannerPath = bannerPath,
                            company = new
                            {
                                id = company.Id,
                                name = company.Name,
                                description = company.Description,
                                updatedAt = company.UpdatedAt
                            },
                            saveResult = 0,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["InfoMessage"] = "No changes detected!";
                }

                return RedirectToAction("Profile", new { tab = "basic" });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error saving company information",
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }

                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Profile", new { tab = "basic" });
            }
        }

        // POST: Employer/Company/UpdateOrganizationInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrganizationInfo(CompanySetupOrganizationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Validation failed", errors = errors });
                }
                return RedirectToAction("Profile", new { tab = "organization", error = "Invalid input data" });
            }

            try
            {
                // Get the current user's company
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                if (company == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Company not found" });
                    }
                    return RedirectToAction("SetupBasic");
                }

                // Kiểm tra các trường thay đổi và cập nhật chỉ khi có thay đổi
                bool hasChanges = false;

                // Update CategoryId if changed
                if (company.CategoryId != model.CategoryId)
                {
                    company.CategoryId = model.CategoryId;
                    hasChanges = true;
                }

                // Update Industry if changed
                if (!string.IsNullOrEmpty(model.Industry) && company.Industry != model.Industry)
                {
                    company.Industry = model.Industry;
                    hasChanges = true;
                }

                // Update EmployeeCount if changed
                if (company.EmployeeCount != model.EmployeeCount)
                {
                    company.EmployeeCount = model.EmployeeCount;
                    hasChanges = true;
                }

                // Update FoundedDate if changed
                if (model.FoundedDate.HasValue && company.FoundedDate != model.FoundedDate)
                {
                    company.FoundedDate = model.FoundedDate;
                    hasChanges = true;
                }

                // Update Website if changed
                if (model.Website != company.Website)
                {
                    company.Website = model.Website;
                    hasChanges = true;
                }

                // Update Vision if changed
                if (model.Vision != company.Vision)
                {
                    company.Vision = model.Vision;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    company.UpdatedAt = DateTime.UtcNow;

                    // Save changes
                    await _companyRepository.UpdateAsync(company);
                    int saveResult = await _companyRepository.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Company organization information updated successfully!",
                            company = new
                            {
                                id = company.Id,
                                categoryId = company.CategoryId,
                                industry = company.Industry,
                                employeeCount = company.EmployeeCount,
                                foundedDate = company.FoundedDate,
                                website = company.Website,
                                vision = company.Vision,
                                updatedAt = company.UpdatedAt
                            },
                            saveResult = saveResult,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["SuccessMessage"] = "Company organization information updated successfully!";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "No changes detected!",
                            company = new
                            {
                                id = company.Id,
                                categoryId = company.CategoryId,
                                industry = company.Industry,
                                employeeCount = company.EmployeeCount,
                                foundedDate = company.FoundedDate,
                                website = company.Website,
                                vision = company.Vision,
                                updatedAt = company.UpdatedAt
                            },
                            saveResult = 0,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["InfoMessage"] = "No changes detected!";
                }

                return RedirectToAction("Profile", new { tab = "organization" });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error saving company information",
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }

                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Profile", new { tab = "organization" });
            }
        }

        // POST: Employer/Company/UpdateSocialInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSocialInfo(CompanySetupSocialViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Validation failed", errors = errors });
                }
                return RedirectToAction("Profile", new { tab = "social", error = "Invalid input data" });
            }

            try
            {
                // Get the current user's company
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                if (company == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Company not found" });
                    }
                    return RedirectToAction("SetupBasic");
                }

                // Kiểm tra xem liệu có thay đổi nào không
                bool hasChanges = false;
                var updatedLinks = new List<object>();

                // Đảm bảo SocialLinks đã được khởi tạo
                if (company.SocialLinks == null)
                {
                    company.SocialLinks = new List<SocialLink>();
                    hasChanges = true;
                }

                // So sánh danh sách liên kết cũ và mới
                // Đối với trường hợp social links, thường cách đơn giản nhất là xóa tất cả và tạo lại
                // Vì việc phát hiện chính xác sự thay đổi khá phức tạp
                if (model.SocialLinks != null)
                {
                    // Chuyển đổi ICollection thành List để có thể truy cập bằng index
                    var socialLinksList = company.SocialLinks.ToList();

                    // Kiểm tra số lượng links
                    if (socialLinksList.Count != model.SocialLinks.Count)
                    {
                        hasChanges = true;
                    }
                    else
                    {
                        // Kiểm tra từng link có thay đổi không
                        for (int i = 0; i < model.SocialLinks.Count; i++)
                        {
                            var newLink = model.SocialLinks[i];
                            // Nếu i vượt quá số phần tử trong danh sách cũ hoặc link bị thay đổi
                            if (i >= socialLinksList.Count ||
                                socialLinksList[i].Platform != newLink.Platform ||
                                socialLinksList[i].Url != newLink.Url)
                            {
                                hasChanges = true;
                                break;
                            }
                        }
                    }

                    if (hasChanges)
                    {
                        // Xóa tất cả liên kết hiện tại
                        company.SocialLinks.Clear();

                        // Thêm các liên kết mới
                        foreach (var link in model.SocialLinks)
                        {
                            if (!string.IsNullOrEmpty(link.Platform) && !string.IsNullOrEmpty(link.Url))
                            {
                                var socialLink = new SocialLink
                                {
                                    Platform = link.Platform,
                                    Url = link.Url,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                company.SocialLinks.Add(socialLink);
                                updatedLinks.Add(new
                                {
                                    platform = socialLink.Platform,
                                    url = socialLink.Url
                                });
                            }
                        }
                    }
                    else
                    {
                        // Nếu không có thay đổi, vẫn cần tạo danh sách để trả về cho client
                        foreach (var link in company.SocialLinks)
                        {
                            updatedLinks.Add(new
                            {
                                platform = link.Platform,
                                url = link.Url
                            });
                        }
                    }
                }

                if (hasChanges)
                {
                    company.UpdatedAt = DateTime.UtcNow;

                    // Save changes
                    await _companyRepository.UpdateAsync(company);
                    int saveResult = await _companyRepository.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Company social information updated successfully!",
                            socialLinks = updatedLinks,
                            company = new
                            {
                                id = company.Id,
                                updatedAt = company.UpdatedAt,
                                socialLinkCount = company.SocialLinks.Count
                            },
                            saveResult = saveResult,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["SuccessMessage"] = "Company social information updated successfully!";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "No changes detected!",
                            socialLinks = updatedLinks,
                            company = new
                            {
                                id = company.Id,
                                updatedAt = company.UpdatedAt,
                                socialLinkCount = company.SocialLinks.Count
                            },
                            saveResult = 0,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["InfoMessage"] = "No changes detected!";
                }

                return RedirectToAction("Profile", new { tab = "social" });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error saving company information",
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }

                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Profile", new { tab = "social" });
            }
        }

        // POST: Employer/Company/UpdateContactInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactInfo(CompanySetupContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Validation failed", errors = errors });
                }
                return RedirectToAction("Profile", new { tab = "contact", error = "Invalid input data" });
            }

            try
            {
                // Get the current user's company
                var user = await _userManager.GetUserAsync(User);
                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);

                if (company == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Company not found" });
                    }
                    return RedirectToAction("SetupBasic");
                }

                // Kiểm tra các trường thay đổi và cập nhật chỉ khi có thay đổi
                bool hasChanges = false;

                // Update Location if changed
                if (!string.IsNullOrEmpty(model.Location) && company.Location != model.Location)
                {
                    company.Location = model.Location;
                    hasChanges = true;
                }

                // Update Phone if changed
                if (!string.IsNullOrEmpty(model.Phone) && company.Phone != model.Phone)
                {
                    company.Phone = model.Phone;
                    hasChanges = true;
                }

                // Update Email if changed
                if (!string.IsNullOrEmpty(model.Email) && company.Email != model.Email)
                {
                    company.Email = model.Email;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    company.UpdatedAt = DateTime.UtcNow;

                    // Save changes
                    await _companyRepository.UpdateAsync(company);
                    int saveResult = await _companyRepository.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Company contact information updated successfully!",
                            company = new
                            {
                                id = company.Id,
                                location = company.Location,
                                phone = company.Phone,
                                email = company.Email,
                                updatedAt = company.UpdatedAt
                            },
                            saveResult = saveResult,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["SuccessMessage"] = "Company contact information updated successfully!";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "No changes detected!",
                            company = new
                            {
                                id = company.Id,
                                location = company.Location,
                                phone = company.Phone,
                                email = company.Email,
                                updatedAt = company.UpdatedAt
                            },
                            saveResult = 0,
                            hasChanges = hasChanges
                        });
                    }

                    TempData["InfoMessage"] = "No changes detected!";
                }

                return RedirectToAction("Profile", new { tab = "contact" });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error saving company information",
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }

                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Profile", new { tab = "contact" });
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