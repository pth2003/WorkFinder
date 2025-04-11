using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.DTOs.JobApplications;
using WorkFinder.Web.DTOs.Jobs;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Route("[controller]")]
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IResumeRepository _resumeRepository;
        private readonly IAuthService _authService;

        public JobController(
            IJobRepository jobRepository,
            ICategoryRepository categoryRepository,
            IResumeRepository resumeRepository,
            IAuthService authService)
        {
            _jobRepository = jobRepository;
            _categoryRepository = categoryRepository;
            _resumeRepository = resumeRepository;
            _authService = authService;
        }
        // get all jobs with advanced filter
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(
                string keyword = "",
                string location = "",
                int? categoryId = null,
                string jobType = null,
                string experienceLevel = null,
                decimal? minSalary = null,
                decimal? maxSalary = null,
                string jobLevel = null,
                DateTime? postedAfter = null,
                int? companyId = null,
                int page = 1,
                int pageSize = 12)
        {
            // Xử lý keyword để tìm kiếm thông minh hơn
            var searchKeywords = keyword?.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Chuyển đổi string thành enum nếu có giá trị
            JobType? jobTypeEnum = null;
            if (!string.IsNullOrEmpty(jobType) && Enum.TryParse<JobType>(jobType.Replace(" ", ""), out var parsedJobType))
            {
                jobTypeEnum = parsedJobType;
            }
            // Nếu từ khóa tìm kiếm có chứa các job type phổ biến, tự động thêm vào bộ lọc
            else if (searchKeywords != null && !string.IsNullOrEmpty(keyword))
            {
                var commonJobTypes = new Dictionary<string, JobType>(StringComparer.OrdinalIgnoreCase) {
                    { "fulltime", JobType.FullTime },
                    { "full-time", JobType.FullTime },
                    { "parttime", JobType.PartTime },
                    { "part-time", JobType.PartTime },
                    { "intern", JobType.Internship },
                    { "internship", JobType.Internship },
                    { "remote", JobType.Remote },
                    { "freelance", JobType.Freelance },
                    { "contract", JobType.Contract }
                };

                foreach (var kw in searchKeywords)
                {
                    if (commonJobTypes.TryGetValue(kw, out var type))
                    {
                        jobTypeEnum = type;
                        // Loại bỏ job type ra khỏi từ khóa tìm kiếm để tránh trùng lặp
                        keyword = string.Join(" ", searchKeywords.Where(k => !k.Equals(kw, StringComparison.OrdinalIgnoreCase)));
                        break;
                    }
                }
            }

            ExperienceLevel? experienceLevelEnum = null;
            if (!string.IsNullOrEmpty(experienceLevel) && Enum.TryParse<ExperienceLevel>(experienceLevel.Replace(" ", ""), out var parsedExperienceLevel))
            {
                experienceLevelEnum = parsedExperienceLevel;
            }

            // Tự động phát hiện mức lương từ từ khóa nếu minSalary và maxSalary không được chỉ định
            if ((!minSalary.HasValue || !maxSalary.HasValue) && searchKeywords != null)
            {
                foreach (var kw in searchKeywords)
                {
                    // Tìm các từ khóa liên quan đến lương
                    if (decimal.TryParse(kw.Replace("k", "000").Replace("$", ""), out var salary))
                    {
                        if (!minSalary.HasValue)
                            minSalary = salary * 0.8m; // 20% dưới giá trị tìm thấy

                        if (!maxSalary.HasValue)
                            maxSalary = salary * 1.2m; // 20% trên giá trị tìm thấy

                        // Loại bỏ keyword lương để tránh lặp lại trong tìm kiếm
                        keyword = string.Join(" ", searchKeywords.Where(k => !k.Equals(kw, StringComparison.OrdinalIgnoreCase)));
                        break;
                    }
                }
            }

            // Thực hiện tìm kiếm nâng cao từ repository
            var result = await _jobRepository.GetJobsAdvancedPagedAsync(
                keyword: keyword,
                location: location,
                categoryId: categoryId,
                jobType: jobTypeEnum,
                experienceLevel: experienceLevelEnum,
                minSalary: minSalary,
                maxSalary: maxSalary,
                jobLevel: experienceLevelEnum,
                postedAfter: postedAfter,
                page: page,
                pageSize: pageSize
            );

            // Chuyển đổi sang DTO
            var jobDtos = result.Jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                JobType = j.JobType.ToString(),
                ExperienceLevelName = j.ExperienceLevel.ToString(),
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive,
                CompanyId = j.CompanyId,
                CompanyName = j.Company?.Name,
                Logo = j.Company?.Logo
            }).ToList();

            // Lấy danh sách categories cho dropdown
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories;

            // Tính toán thông tin phân trang
            var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

            // Truyền thông tin phân trang và bộ lọc vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalJobs = result.TotalCount;
            ViewBag.Keyword = keyword;
            ViewBag.Location = location;
            ViewBag.CategoryId = categoryId;
            ViewBag.JobType = jobType;
            ViewBag.ExperienceLevel = experienceLevel;
            ViewBag.MinSalary = minSalary;
            ViewBag.MaxSalary = maxSalary;
            ViewBag.JobLevel = jobLevel;
            ViewBag.PostedAfter = postedAfter;

            return View(jobDtos);
        }

        // get job details by id
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobRepository.GetJobWithDetailsAsync(id);
            if (job == null)
                return NotFound();
                
            // Nếu job có slug, chuyển hướng đến URL thân thiện với SEO
            if (!string.IsNullOrEmpty(job.Slug))
            {
                return RedirectToActionPermanent(nameof(DetailsBySlug), new { slug = job.Slug });
            }

            // Nếu không có slug, hiển thị chi tiết như bình thường
            var relatedJobs = await _jobRepository.GetRelatedJobsAsync(job.CompanyId);
            var relatedJobDtos = relatedJobs.Where(j => j.Id != id).Take(3).Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                JobType = j.JobType.ToString(),
                ExperienceLevelName = j.ExperienceLevel.ToString(),
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive,
                CompanyId = j.CompanyId,
                CompanyName = j.Company?.Name,
                Logo = j.Company?.Logo
            }).ToList();

            // Lấy thông tin resume và kiểm tra nếu đã apply
            Resume? userResume = null;
            bool hasApplied = false;
            DateTime? previouslyAppliedDate = null;
            
            if (User.Identity.IsAuthenticated)
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    userResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);
                    hasApplied = await _jobRepository.HasUserAppliedToJobAsync(id, user.Id);
                    
                    // Nếu đã apply, lấy thời gian apply gần nhất
                    if (hasApplied)
                    {
                        var application = await _jobRepository.GetUserLatestApplicationAsync(id, user.Id);
                        if (application != null)
                        {
                            previouslyAppliedDate = application.AppliedDate;
                            
                            // Kiểm tra xem đã qua 1 ngày chưa
                            var daysSinceLastApply = (DateTime.UtcNow - application.AppliedDate).TotalDays;
                            ViewBag.DaysSinceLastApply = daysSinceLastApply;
                            ViewBag.CanReapply = daysSinceLastApply >= 1;
                        }
                    }
                }
            }

            // Tạo ViewModel
            var viewModel = new JobDetailViewModel
            {
                // Thông tin công việc
                Id = job.Id,
                Title = job.Title,
                Slug = job.Slug,
                Description = job.Description,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                JobType = job.JobType.ToString(),
                ExperienceLevelName = job.ExperienceLevel.ToString(),
                ExpiryDate = job.ExpiryDate,
                IsActive = job.IsActive,
                CreatedAt = job.CreatedAt,
                PostedDate = job.CreatedAt,

                // Thông tin danh mục
                Categories = job.Categories.Select(c => c.Category.Name).ToList(),

                // Thông tin công ty
                CompanyId = job.CompanyId,
                CompanyName = job.Company.Name,
                CompanyLogo = job.Company.Logo,
                CompanyDescription = job.Company.Description,
                CompanyWebsite = job.Company.Website,
                CompanyLocation = job.Company.Location,
                CompanyEmployeeCount = job.Company.EmployeeCount,
                CompanyIndustry = job.Company.Industry,
                CompanyIsVerified = job.Company.IsVerified,
                Email = "career@" + job.Company.Website.Replace("https://", "").Replace("http://", ""),
                Phone = "(406) 555-0120", // Giả định hoặc lấy từ thông tin công ty

                // Job liên quan
                RelatedJobs = relatedJobDtos,

                // Resume hiện có
                UserResume = userResume,
                
                // Thông tin apply
                HasApplied = hasApplied,
                PreviouslyAppliedDate = previouslyAppliedDate
            };
            return View(viewModel);
        }

        // get job details by slug
        [HttpGet("details/{slug}")]
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            var job = await _jobRepository.GetJobBySlugAsync(slug);
            if (job == null)
                return NotFound();

            var relatedJobs = await _jobRepository.GetRelatedJobsAsync(job.CompanyId);
            var relatedJobDtos = relatedJobs.Where(j => j.Id != job.Id).Take(3).Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                JobType = j.JobType.ToString(),
                ExperienceLevelName = j.ExperienceLevel.ToString(),
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive,
                CompanyId = j.CompanyId,
                CompanyName = j.Company?.Name,
                Logo = j.Company?.Logo
            }).ToList();

            // Lấy thông tin resume và kiểm tra nếu đã apply
            Resume? userResume = null;
            bool hasApplied = false;
            DateTime? previouslyAppliedDate = null;
            
            if (User.Identity.IsAuthenticated)
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    userResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);
                    hasApplied = await _jobRepository.HasUserAppliedToJobAsync(job.Id, user.Id);
                    
                    // Nếu đã apply, lấy thời gian apply gần nhất
                    if (hasApplied)
                    {
                        var application = await _jobRepository.GetUserLatestApplicationAsync(job.Id, user.Id);
                        if (application != null)
                        {
                            previouslyAppliedDate = application.AppliedDate;
                            
                            // Kiểm tra xem đã qua 1 ngày chưa
                            var daysSinceLastApply = (DateTime.UtcNow - application.AppliedDate).TotalDays;
                            ViewBag.DaysSinceLastApply = daysSinceLastApply;
                            ViewBag.CanReapply = daysSinceLastApply >= 1;
                        }
                    }
                }
            }

            // Tạo ViewModel
            var viewModel = new JobDetailViewModel
            {
                // Thông tin công việc
                Id = job.Id,
                Title = job.Title,
                Slug = job.Slug,
                Description = job.Description,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                JobType = job.JobType.ToString(),
                ExperienceLevelName = job.ExperienceLevel.ToString(),
                ExpiryDate = job.ExpiryDate,
                IsActive = job.IsActive,
                CreatedAt = job.CreatedAt,
                PostedDate = job.CreatedAt,

                // Thông tin danh mục
                Categories = job.Categories.Select(c => c.Category.Name).ToList(),

                // Thông tin công ty
                CompanyId = job.CompanyId,
                CompanyName = job.Company.Name,
                CompanyLogo = job.Company.Logo,
                CompanyDescription = job.Company.Description,
                CompanyWebsite = job.Company.Website,
                CompanyLocation = job.Company.Location,
                CompanyEmployeeCount = job.Company.EmployeeCount,
                CompanyIndustry = job.Company.Industry,
                CompanyIsVerified = job.Company.IsVerified,
                Email = "career@" + job.Company.Website.Replace("https://", "").Replace("http://", ""),
                Phone = "(406) 555-0120", // Giả định hoặc lấy từ thông tin công ty

                // Job liên quan
                RelatedJobs = relatedJobDtos,

                // Resume hiện có
                UserResume = userResume,
                
                // Thông tin apply
                HasApplied = hasApplied,
                PreviouslyAppliedDate = previouslyAppliedDate
            };
            return View("Details", viewModel);
        }

        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập để gửi application
        [Route("Apply/{id}")]
        public async Task<IActionResult> Apply(int id, JobApplicationDto model)
        {
            if (!ModelState.IsValid)
            {
                // Return to the job details page with validation errors
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Lấy thông tin user hiện tại
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify user. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }

            int applicantId = user.Id;

            // Kiểm tra xem user đã apply job này chưa bằng repository
            bool hasApplied = await _jobRepository.HasUserAppliedToJobAsync(id, applicantId);
            
            if (hasApplied)
            {
                // Kiểm tra thời gian apply gần nhất
                var previousApplication = await _jobRepository.GetUserLatestApplicationAsync(id, applicantId);
                if (previousApplication != null)
                {
                    var daysSinceLastApply = (DateTime.UtcNow - previousApplication.AppliedDate).TotalDays;
                    
                    // Nếu chưa đủ 1 ngày, không cho apply lại
                    if (daysSinceLastApply < 1)
                    {
                        var hoursRemaining = (24 - (DateTime.UtcNow - previousApplication.AppliedDate).TotalHours);
                        TempData["ErrorMessage"] = $"You can apply again after 24 hours. Please wait {hoursRemaining:F1} more hour(s).";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    
                    // Nếu đủ 1 ngày, cập nhật application cũ
                    previousApplication.CoverLetter = model.CoverLetter;
                    previousApplication.UpdatedAt = DateTime.UtcNow;
                    previousApplication.AppliedDate = DateTime.UtcNow;
                    previousApplication.Status = ApplicationStatus.Applied; // Reset status to Applied
                    
                    await _jobRepository.UpdateJobApplicationAsync(previousApplication);
                    
                    TempData["SuccessMessage"] = "Your application has been updated successfully!";
                    
                    // Lấy job để kiểm tra xem có slug không
                    var jobFound = await _jobRepository.GetJobWithDetailsAsync(id);
                    if (jobFound != null && !string.IsNullOrEmpty(jobFound.Slug))
                    {
                        // Nếu có slug, chuyển hướng đến URL thân thiện với SEO
                        return RedirectToAction(nameof(DetailsBySlug), new { slug = jobFound.Slug });
                    }
                    
                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            // Xác định đường dẫn đến resume
            string resumeFileUrl;

            if (model.UseExistingResume)
            {
                // Sử dụng resume đã có
                var existingResume = await _resumeRepository.GetResumeByUserIdAsync(applicantId);
                if (existingResume == null || string.IsNullOrEmpty(existingResume.FileUrl))
                {
                    TempData["ErrorMessage"] = "No resume found. Please upload a resume.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                resumeFileUrl = existingResume.FileUrl;
            }
            else
            {
                // Upload resume mới
                if (model.ResumeFile == null || model.ResumeFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Resume file is required.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Tạo tên file duy nhất bằng cách kết hợp GUID với tên file gốc
                // Điều này ngăn chặn xung đột tên file khi nhiều người dùng tải lên file có cùng tên
                // GUID đảm bảo mỗi file tải lên có tên duy nhất, ngay cả khi tên file gốc giống nhau
                // Ví dụ: "a1b2c3d4-5678-90ab-cdef-1234567890ab_resume.pdf"
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ResumeFile.FileName)}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(fileStream);
                }

                resumeFileUrl = $"/uploads/resumes/{fileName}";

                // Kiểm tra xem user đã có resume chưa, nếu chưa thì tạo mới
                if (!await _resumeRepository.UserHasResumeAsync(applicantId))
                {
                    var resume = new Resume
                    {
                        UserId = applicantId,
                        FileUrl = resumeFileUrl,
                        Summary = "", // Default empty values
                        Skills = "",
                        Education = "",
                        Experience = "",
                        Certifications = "",
                        Languages = ""
                    };

                    await _resumeRepository.AddResumeAsync(resume);
                }
                else
                {
                    // Cập nhật FileUrl cho resume hiện có
                    var existingResume = await _resumeRepository.GetResumeByUserIdAsync(applicantId);
                    if (existingResume != null)
                    {
                        existingResume.FileUrl = resumeFileUrl;
                        await _resumeRepository.UpdateAsync(existingResume);
                        await _resumeRepository.SaveChangesAsync();
                    }
                }
            }

            // Tạo job application mới
            var jobApplication = new JobApplication
            {
                JobId = id,
                ApplicantId = applicantId,
                CoverLetter = model.CoverLetter,
                Status = ApplicationStatus.Applied,
                AppliedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Lưu job application vào database
            await _jobRepository.AddJobApplicationAsync(jobApplication);

            // Success message and redirect
            TempData["SuccessMessage"] = "Your application has been submitted successfully!";
            
            // Lấy job để kiểm tra xem có slug không
            var jobFound = await _jobRepository.GetJobWithDetailsAsync(id);
            if (jobFound != null && !string.IsNullOrEmpty(jobFound.Slug))
            {
                // Nếu có slug, chuyển hướng đến URL thân thiện với SEO
                return RedirectToAction(nameof(DetailsBySlug), new { slug = jobFound.Slug });
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập để gửi application
        [Route("Apply/s/{slug}")]
        public async Task<IActionResult> ApplyBySlug(string slug, JobApplicationDto model)
        {
            if (!ModelState.IsValid)
            {
                // Return to the job details page with validation errors
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return RedirectToAction(nameof(DetailsBySlug), new { slug });
            }

            // Lấy thông tin user hiện tại
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify user. Please try again.";
                return RedirectToAction(nameof(DetailsBySlug), new { slug });
            }

            // Lấy job từ slug
            var jobInfo = await _jobRepository.GetJobBySlugAsync(slug);
            if (jobInfo == null)
            {
                TempData["ErrorMessage"] = "Job not found.";
                return RedirectToAction(nameof(Index));
            }

            int applicantId = user.Id;
            int jobId = jobInfo.Id;

            // Kiểm tra xem user đã apply job này chưa bằng repository
            bool hasApplied = await _jobRepository.HasUserAppliedToJobAsync(jobId, applicantId);
            
            if (hasApplied)
            {
                // Kiểm tra thời gian apply gần nhất
                var previousApplication = await _jobRepository.GetUserLatestApplicationAsync(jobId, applicantId);
                if (previousApplication != null)
                {
                    var daysSinceLastApply = (DateTime.UtcNow - previousApplication.AppliedDate).TotalDays;
                    
                    // Nếu chưa đủ 1 ngày, không cho apply lại
                    if (daysSinceLastApply < 1)
                    {
                        var hoursRemaining = (24 - (DateTime.UtcNow - previousApplication.AppliedDate).TotalHours);
                        TempData["ErrorMessage"] = $"You can apply again after 24 hours. Please wait {hoursRemaining:F1} more hour(s).";
                        return RedirectToAction(nameof(DetailsBySlug), new { slug });
                    }
                    
                    // Nếu đủ 1 ngày, cập nhật application cũ
                    previousApplication.CoverLetter = model.CoverLetter;
                    previousApplication.UpdatedAt = DateTime.UtcNow;
                    previousApplication.AppliedDate = DateTime.UtcNow;
                    previousApplication.Status = ApplicationStatus.Applied; // Reset status to Applied
                    
                    await _jobRepository.UpdateJobApplicationAsync(previousApplication);
                    
                    TempData["SuccessMessage"] = "Your application has been updated successfully!";
                    return RedirectToAction(nameof(DetailsBySlug), new { slug });
                }
            }

            // Xác định đường dẫn đến resume
            string resumeFileUrl;

            if (model.UseExistingResume)
            {
                // Sử dụng resume đã có
                var existingResume = await _resumeRepository.GetResumeByUserIdAsync(applicantId);
                if (existingResume == null || string.IsNullOrEmpty(existingResume.FileUrl))
                {
                    TempData["ErrorMessage"] = "No resume found. Please upload a resume.";
                    return RedirectToAction(nameof(DetailsBySlug), new { slug });
                }

                resumeFileUrl = existingResume.FileUrl;
            }
            else
            {
                // Upload resume mới
                if (model.ResumeFile == null || model.ResumeFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Resume file is required.";
                    return RedirectToAction(nameof(DetailsBySlug), new { slug });
                }

                // Tạo tên file duy nhất bằng cách kết hợp GUID với tên file gốc
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ResumeFile.FileName)}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(fileStream);
                }

                resumeFileUrl = $"/uploads/resumes/{fileName}";

                // Kiểm tra xem user đã có resume chưa, nếu chưa thì tạo mới
                if (!await _resumeRepository.UserHasResumeAsync(applicantId))
                {
                    var resume = new Resume
                    {
                        UserId = applicantId,
                        FileUrl = resumeFileUrl,
                        Summary = "", // Default empty values
                        Skills = "",
                        Education = "",
                        Experience = "",
                        Certifications = "",
                        Languages = ""
                    };

                    await _resumeRepository.AddResumeAsync(resume);
                }
                else
                {
                    // Cập nhật FileUrl cho resume hiện có
                    var existingResume = await _resumeRepository.GetResumeByUserIdAsync(applicantId);
                    if (existingResume != null)
                    {
                        existingResume.FileUrl = resumeFileUrl;
                        await _resumeRepository.UpdateAsync(existingResume);
                        await _resumeRepository.SaveChangesAsync();
                    }
                }
            }

            // Tạo job application
            var jobApplication = new JobApplication
            {
                JobId = jobId,
                ApplicantId = applicantId,
                CoverLetter = model.CoverLetter,
                Status = ApplicationStatus.Applied,
                AppliedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Lưu job application vào database
            await _jobRepository.AddJobApplicationAsync(jobApplication);

            // Success message and redirect
            TempData["SuccessMessage"] = "Your application has been submitted successfully!";
            
            // Không cần phải check lại slug, vì đã có slug sẵn trong tham số
            return RedirectToAction(nameof(DetailsBySlug), new { slug });
        }

        // get job by company id
        [HttpGet("company/{id}")]
        public async Task<IActionResult> CompanyJobs(int id)
        {
            var jobs = await _jobRepository.GetJobsByCompanyAsync(id);
            var jobDtos = jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                JobType = j.JobType.ToString(),
                ExperienceLevelName = j.ExperienceLevel.ToString(),
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive
            }).ToList();
            return View(jobDtos);
        }

        // AJAX endpoint for job filtering
        [HttpGet("JobList")]
        public async Task<IActionResult> JobList(
            string keyword = "",
            string location = "",
            int? categoryId = null,
            string jobType = null,
            string experienceLevel = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            string jobLevel = null,
            DateTime? postedAfter = null,
            int? companyId = null,
            int page = 1,
            int pageSize = 12)
        {
            // Convert strings to enums if available
            JobType? jobTypeEnum = null;
            if (!string.IsNullOrEmpty(jobType) && Enum.TryParse<JobType>(jobType.Replace(" ", ""), out var parsedJobType))
            {
                jobTypeEnum = parsedJobType;
            }

            ExperienceLevel? experienceLevelEnum = null;
            if (!string.IsNullOrEmpty(experienceLevel))
            {
                var firstWord = experienceLevel.Split(' ')[0];
                if (Enum.TryParse<ExperienceLevel>(firstWord, out var parsedExpLevel))
                {
                    experienceLevelEnum = parsedExpLevel;
                }
            }

            ExperienceLevel? jobLevelEnum = null;
            if (!string.IsNullOrEmpty(jobLevel))
            {
                var firstWord = jobLevel.Split(' ')[0];
                if (Enum.TryParse<ExperienceLevel>(firstWord, out var parsedJobLevel))
                {
                    jobLevelEnum = parsedJobLevel;
                }
            }

            // Set DateTime to UTC if provided
            if (postedAfter.HasValue)
            {
                postedAfter = DateTime.SpecifyKind(postedAfter.Value, DateTimeKind.Utc);
            }

            // Declare variables to store results
            IEnumerable<Job> jobs;
            int totalCount;

            if (companyId.HasValue)
            {
                // If companyId is provided, get jobs for that company
                var companyJobs = await _jobRepository.GetJobsByCompanyAsync(companyId.Value);
                jobs = companyJobs;
                totalCount = companyJobs.Count();

                // Set ViewBag data for company filter
                var company = await _jobRepository.GetCompanyByIdAsync(companyId.Value);
                if (company != null)
                {
                    ViewBag.CompanyName = company.Name;
                    ViewBag.CompanyLogo = company.Logo;
                }
                ViewBag.CompanyId = companyId.Value;
            }
            else
            {
                // Otherwise use the advanced filter
                (jobs, totalCount) = await _jobRepository.GetJobsAdvancedPagedAsync(
                    keyword, location, categoryId, jobTypeEnum, experienceLevelEnum,
                    minSalary, maxSalary, jobLevelEnum, postedAfter, page, pageSize);
            }

            // Convert to DTOs
            var jobDtos = jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                JobType = j.JobType.ToString(),
                ExperienceLevelName = j.ExperienceLevel.ToString(),
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive,
                CompanyId = j.CompanyId,
                CompanyName = j.Company?.Name,
                Logo = j.Company?.Logo
            }).ToList();

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Set ViewBag properties for the partial view
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalJobs = totalCount;

            // Return partial view
            return PartialView("_JobListPartial", jobDtos);
        }
    }
}