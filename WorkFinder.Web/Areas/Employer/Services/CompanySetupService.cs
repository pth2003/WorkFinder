using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using WorkFinder.Web.Areas.Employer.Models;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Areas.Employer.Services
{
    public class CompanySetupService : ICompanySetupService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const string BasicInfoKey = "CompanySetupBasicInfo";
        private const string OrganizationInfoKey = "CompanySetupOrganizationInfo";
        private const string SocialInfoKey = "CompanySetupSocialInfo";
        private const string ContactInfoKey = "CompanySetupContactInfo";

        public CompanySetupService(
            IHttpContextAccessor httpContextAccessor,
            ICompanyRepository companyRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _companyRepository = companyRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SaveBasicInfoAsync(CompanySetupBasicViewModel model)
        {
            try
            {
                // Xử lý file uploads trước - chỉ xử lý đường dẫn file
                string logoPath = model.LogoPath;
                string bannerPath = model.BannerPath;

                if (model.Logo != null && model.Logo.Length > 0)
                {
                    // Kiểm tra kích thước (max 5MB)
                    if (model.Logo.Length > 5 * 1024 * 1024)
                    {
                        throw new InvalidOperationException("Logo file size must be less than 5MB");
                    }
                    logoPath = await SaveFileAsync(model.Logo, "logos");
                }

                if (model.Banner != null && model.Banner.Length > 0)
                {
                    // Kiểm tra kích thước (max 5MB)
                    if (model.Banner.Length > 5 * 1024 * 1024)
                    {
                        throw new InvalidOperationException("Banner file size must be less than 5MB");
                    }
                    bannerPath = await SaveFileAsync(model.Banner, "banners");
                }

                // Tạo bản sao của model không có property IFormFile để serialize
                var sessionModel = new CompanySetupBasicViewModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    LogoPath = logoPath ?? model.LogoPath,
                    BannerPath = bannerPath ?? model.BannerPath
                };

                // Lưu vào session
                SetSessionData(BasicInfoKey, sessionModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveBasicInfoAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task SaveOrganizationInfoAsync(CompanySetupOrganizationViewModel model)
        {
            // Lưu vào session
            SetSessionData(OrganizationInfoKey, model);
        }

        public async Task SaveSocialInfoAsync(CompanySetupSocialViewModel model)
        {
            // Lưu vào session
            SetSessionData(SocialInfoKey, model);
        }

        public async Task<Company> CompleteSetupAsync(CompanySetupContactViewModel model)
        {
            // Lưu thông tin contact vào session
            SetSessionData(ContactInfoKey, model);

            // Lấy người dùng hiện tại
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Lấy hoặc tạo mới công ty
            var company = await _companyRepository.GetByOwnerIdAsync(user.Id) ?? new Company { OwnerId = user.Id };

            // Lấy thông tin đã lưu trong session
            var basicInfo = GetBasicInfo();
            var organizationInfo = GetOrganizationInfo();
            var socialInfo = GetSocialInfo();

            // Cập nhật thông tin công ty từ các bước trước
            // Bước 1: Basic Info
            company.Name = basicInfo.Name;
            company.Description = basicInfo.Description;
            company.Logo = basicInfo.LogoPath;
            company.Banner = basicInfo.BannerPath;

            // Bước 2: Organization Info
            company.CategoryId = organizationInfo.CategoryId;
            company.Industry = organizationInfo.Industry;
            company.EmployeeCount = organizationInfo.EmployeeCount;

            // Fix cho lỗi DateTime với PostgreSQL - đảm bảo ngày giờ ở định dạng UTC
            if (organizationInfo.FoundedDate.HasValue)
            {
                // Chuyển đổi datetime sang UTC trước khi lưu
                DateTime foundedDateUtc;
                if (organizationInfo.FoundedDate.Value.Kind == DateTimeKind.Unspecified)
                {
                    // Với datetime không có Kind, chuyển đổi sang UTC
                    foundedDateUtc = DateTime.SpecifyKind(organizationInfo.FoundedDate.Value, DateTimeKind.Utc);
                }
                else if (organizationInfo.FoundedDate.Value.Kind == DateTimeKind.Local)
                {
                    // Với datetime ở định dạng Local, chuyển đổi sang UTC
                    foundedDateUtc = organizationInfo.FoundedDate.Value.ToUniversalTime();
                }
                else
                {
                    // Datetime đã ở định dạng UTC
                    foundedDateUtc = organizationInfo.FoundedDate.Value;
                }

                company.FoundedDate = foundedDateUtc;
            }
            else
            {
                company.FoundedDate = null;
            }

            company.Website = organizationInfo.Website;
            company.Vision = organizationInfo.Vision;

            // Bước 3: Xử lý Social Links
            if (socialInfo.SocialLinks != null && socialInfo.SocialLinks.Any())
            {
                // Xóa các social links hiện tại (nếu cần cập nhật)
                if (company.Id > 0 && company.SocialLinks != null)
                {
                    company.SocialLinks.Clear();
                }
                else
                {
                    company.SocialLinks = new List<SocialLink>();
                }

                // Thêm các social links mới từ viewmodel
                foreach (var link in socialInfo.SocialLinks)
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
                    }
                }
            }

            // Bước 4: Contact Info
            company.Location = model.Location;
            company.Phone = model.Phone;
            company.Email = model.Email;

            // Đảm bảo thời gian tạo và cập nhật đều ở UTC
            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            // Lưu công ty vào database
            await _companyRepository.AddAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Xóa dữ liệu session sau khi đã hoàn tất
            ClearSessionData();

            return company;
        }

        public CompanySetupBasicViewModel GetBasicInfo()
        {
            return GetSessionData<CompanySetupBasicViewModel>(BasicInfoKey) ?? new CompanySetupBasicViewModel();
        }

        public CompanySetupOrganizationViewModel GetOrganizationInfo()
        {
            return GetSessionData<CompanySetupOrganizationViewModel>(OrganizationInfoKey) ?? new CompanySetupOrganizationViewModel();
        }

        public CompanySetupSocialViewModel GetSocialInfo()
        {
            return GetSessionData<CompanySetupSocialViewModel>(SocialInfoKey) ?? new CompanySetupSocialViewModel();
        }

        public CompanySetupContactViewModel GetContactInfo()
        {
            return GetSessionData<CompanySetupContactViewModel>(ContactInfoKey) ?? new CompanySetupContactViewModel();
        }

        public bool HasCompletedBasicInfo()
        {
            var data = GetSessionData<CompanySetupBasicViewModel>(BasicInfoKey);
            return data != null && !string.IsNullOrEmpty(data.Name) && !string.IsNullOrEmpty(data.Description);
        }

        public bool HasCompletedOrganizationInfo()
        {
            return GetSessionData<CompanySetupOrganizationViewModel>(OrganizationInfoKey) != null;
        }

        public bool HasCompletedSocialInfo()
        {
            return _httpContextAccessor.HttpContext.Session.TryGetValue(SocialInfoKey, out _);
        }

        public async Task InitializeWithExistingCompanyAsync(Company company)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Store basic info in session
            var basicModel = new CompanySetupBasicViewModel
            {
                Name = company.Name,
                Description = company.Description,
                LogoPath = company.Logo,
                BannerPath = company.Banner
            };
            SetSessionData(BasicInfoKey, basicModel);

            // Store organization info in session
            var orgModel = new CompanySetupOrganizationViewModel
            {
                CategoryId = company.CategoryId,
                Industry = company.Industry,
                EmployeeCount = company.EmployeeCount,
                FoundedDate = company.FoundedDate,
                Website = company.Website,
                Vision = company.Vision
            };
            SetSessionData(OrganizationInfoKey, orgModel);

            // Store social info in session
            var socialModel = new CompanySetupSocialViewModel
            {
                SocialLinks = company.SocialLinks?.Select(sl => new SocialLinkViewModel
                {
                    Id = sl.Id,
                    Platform = sl.Platform,
                    Url = sl.Url
                }).ToList() ?? new List<SocialLinkViewModel>()
            };
            SetSessionData(SocialInfoKey, socialModel);

            // Store contact info in session
            var contactModel = new CompanySetupContactViewModel
            {
                Location = company.Location,
                Phone = company.Phone,
                Email = company.Email
            };
            SetSessionData(ContactInfoKey, contactModel);
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null) return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }

        private void SetSessionData<T>(string key, T data)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            if (_httpContextAccessor.HttpContext.Session == null)
            {
                throw new InvalidOperationException("Session is not available");
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string serializedData = JsonSerializer.Serialize(data, options);
                Console.WriteLine($"Serialized data for {key}: {serializedData}");
                _httpContextAccessor.HttpContext.Session.SetString(key, serializedData);

                // Kiểm tra xem đã lưu thành công chưa
                string checkData = _httpContextAccessor.HttpContext.Session.GetString(key);
                if (string.IsNullOrEmpty(checkData))
                {
                    throw new InvalidOperationException($"Failed to save data to session for key: {key}");
                }

                Console.WriteLine($"Session data saved successfully for key: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serializing/saving data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private T GetSessionData<T>(string key)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("HttpContext is not available");
                return default;
            }

            if (_httpContextAccessor.HttpContext.Session == null)
            {
                Console.WriteLine("Session is not available");
                return default;
            }

            try
            {
                var data = _httpContextAccessor.HttpContext.Session.GetString(key);
                Console.WriteLine($"Getting session data for key {key}: {(data != null ? "Data found" : "No data")}");

                if (string.IsNullOrEmpty(data))
                {
                    return default;
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                T result = JsonSerializer.Deserialize<T>(data, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data from session: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return default;
            }
        }

        private void ClearSessionData()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.Remove(BasicInfoKey);
                _httpContextAccessor.HttpContext.Session.Remove(OrganizationInfoKey);
                _httpContextAccessor.HttpContext.Session.Remove(SocialInfoKey);
                _httpContextAccessor.HttpContext.Session.Remove(ContactInfoKey);
            }
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            if (_httpContextAccessor.HttpContext?.User.Identity.IsAuthenticated == true)
            {
                return await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            }
            return null;
        }
    }
}