using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using WorkFinder.Web.Areas.Employer.Models;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class JobController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICategoryRepository _categoryRepository;

        public JobController(
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            ICategoryRepository? categoryRepository = null)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _categoryRepository = categoryRepository;
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
                return RedirectToAction("SetupBasic", "Company", new { area = "Employer" });
            }

            var jobs = await _jobRepository.GetJobsByCompanyIdAsync(company.Id, 10);
            return View(jobs);
        }

        public IActionResult Create()
        {
            // Tạo view model mới với giá trị mặc định
            var model = new JobPostViewModel
            {
                ApplyMethod = "Jobpilot" // Mặc định sử dụng Jobpilot
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                return RedirectToAction("SetupBasic", "Company", new { area = "Employer" });
            }

            // Tạo đối tượng Job từ ViewModel với mapping chính xác
            var job = await CreateJobFromViewModel(model, company.Id);

            // Lưu job vào database
            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            // Xử lý thêm danh mục và tags (nếu có repository)
            await ProcessJobCategoriesAndTags(job.Id, model);

            TempData["SuccessMessage"] = "Job posted successfully!";
            return RedirectToAction("Index", "Job", new { area = "Employer" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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

            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu job
            if (job.CompanyId != company.Id)
            {
                return Forbid();
            }

            // Xóa job
            await _jobRepository.DeleteAsync(id);
            await _jobRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        /// <summary>
        /// Tạo đối tượng Job từ ViewModel
        /// </summary>
        private async Task<Job> CreateJobFromViewModel(JobPostViewModel model, int companyId)
        {
            // Chuyển đổi string sang enum và khởi tạo giá trị mặc định nếu cần
            JobType jobType = JobType.FullTime; // Giá trị mặc định
            if (!string.IsNullOrEmpty(model.JobType))
            {
                // Loại bỏ khoảng trắng và chuyển đổi
                if (Enum.TryParse<JobType>(model.JobType.Replace(" ", ""), out var parsedJobType))
                {
                    jobType = parsedJobType;
                }
            }

            ExperienceLevel experienceLevel = ExperienceLevel.Entry; // Giá trị mặc định
            if (!string.IsNullOrEmpty(model.Experience))
            {
                // Lấy từ đầu tiên (Entry, Senior, v.v.)
                var firstWord = model.Experience.Split(' ')[0];
                if (Enum.TryParse<ExperienceLevel>(firstWord, out var parsedExpLevel))
                {
                    experienceLevel = parsedExpLevel;
                }
            }

            // Lưu thông tin vào metadata
            var metadata = new System.Collections.Generic.Dictionary<string, string>();
            if (!string.IsNullOrEmpty(model.Education))
                metadata.Add("Education", model.Education);

            if (!string.IsNullOrEmpty(model.JobLevel))
                metadata.Add("JobLevel", model.JobLevel);

            if (!string.IsNullOrEmpty(model.SalaryType))
                metadata.Add("SalaryType", model.SalaryType);

            if (model.Vacancies.HasValue)
                metadata.Add("Vacancies", model.Vacancies.ToString());

            if (!string.IsNullOrEmpty(model.ApplyMethod))
                metadata.Add("ApplyMethod", model.ApplyMethod);

            // Tạo đối tượng Job từ ViewModel với mapping chính xác
            var job = new Job
            {
                Title = model.Title,
                Description = model.Description,
                Requirements = model.Responsibilities, // Map Responsibilities vào Requirements
                JobType = jobType,
                ExperienceLevel = experienceLevel,
                SalaryMin = model.MinSalary ?? 0,
                SalaryMax = model.MaxSalary ?? 0,
                // Gán các giá trị mặc định nếu không có trong model
                Location = "Remote", // Có thể thêm trường Location vào ViewModel nếu cần
                Benefits = "To be added", // Có thể thêm trường Benefits vào ViewModel nếu cần
                ExpiryDate = model.ExpirationDate.HasValue ? model.ExpirationDate.Value : DateTime.Now.AddMonths(1),
                IsActive = true,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Lưu metadata vào trường mới
                Metadata = JsonSerializer.Serialize(metadata)
            };

            return job;
        }

        /// <summary>
        /// Xử lý danh mục và tags cho job
        /// </summary>
        private async Task ProcessJobCategoriesAndTags(int jobId, JobPostViewModel model)
        {
            // Nếu không có repository, thoát sớm
            if (_categoryRepository == null)
                return;

            try
            {
                // Xử lý JobRole như một danh mục chính
                if (!string.IsNullOrEmpty(model.JobRole))
                {
                    await TryAddCategory(jobId, model.JobRole, $"Jobs for {model.JobRole}");
                }

                // Xử lý Tags nếu có
                if (!string.IsNullOrEmpty(model.Tags))
                {
                    var tags = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tags)
                    {
                        var trimmedTag = tag.Trim();
                        if (!string.IsNullOrEmpty(trimmedTag))
                        {
                            await TryAddCategory(jobId, trimmedTag, $"Jobs tagged with {trimmedTag}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error processing job categories: {ex.Message}");
            }
        }

        /// <summary>
        /// Cố gắng thêm một danh mục và liên kết với job
        /// </summary>
        private async Task TryAddCategory(int jobId, string categoryName, string description)
        {
            try
            {
                // Tìm danh mục theo tên
                var category = await _categoryRepository.GetCategoryByNameAsync(categoryName);

                // Nếu không tồn tại, tạo mới
                if (category == null)
                {
                    category = new Category { Name = categoryName, Description = description };
                    await _categoryRepository.AddAsync(category);
                    await _categoryRepository.SaveChangesAsync();
                }

                // Liên kết job với danh mục
                await _categoryRepository.AddJobCategoryAsync(jobId, category.Id);
            }
            catch (NotImplementedException)
            {
                // Bỏ qua nếu phương thức chưa được implement
            }
            catch (Exception)
            {
                // Bỏ qua lỗi khác
            }
        }

        #endregion
    }
}