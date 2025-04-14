using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WorkFinder.Web.Repositories
{
    public class JobRepository : Repository<Job>, IJobRepository
    {
        private readonly WorkFinderContext _context;
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(WorkFinderContext context, ILogger<JobRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Job>> GetActiveJobsAsync()
        {
            return await _context.Jobs
                .Where(j => j.IsActive && j.ExpiryDate > DateTime.Now)
                .Include(j => j.Company)
                .ToListAsync();
        }

        /// <summary>
        /// Phương thức mới để lấy công việc theo công ty
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Job>> GetJobsByCompanyAsync(int companyId)
        {
            return await _context.Jobs
                .Where(j => j.CompanyId == companyId && j.IsActive)
                .Include(j => j.Company)
                .ToListAsync();
        }

        /// <summary>
        /// Phương thức mới để lấy công việc theo danh mục
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Job>> GetJobsByCategoryAsync(int categoryId)
        {
            return await _context.Jobs
                .Where(j => j.Categories.Any(c => c.CategoryId == categoryId) && j.IsActive)
                .Include(j => j.Company)
                .ToListAsync();
        }
        /// <summary>
        /// Phương thức mới để lấy công việc theo các tiêu chí cơ bản
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="location"></param>
        /// <param name="categoryId"></param>
        /// <param name="jobType"></param>      
        public async Task<IEnumerable<Job>> GetJobsByFilterAsync(
            string keyword,
            string location,
            int? categoryId,
            JobType? jobType,
            ExperienceLevel? experienceLevel,
            decimal? minSalary,
            decimal? maxSalary)
        {
            var query = _context.Jobs.AsQueryable();

            // Apply filters
            query = ApplyFilters(query, keyword, location, categoryId, jobType, experienceLevel, minSalary, maxSalary);

            return await query
                .Include(j => j.Company)
                .Include(j => j.Categories)
                    .ThenInclude(jc => jc.Category)
                .ToListAsync();
        }
        /// <summary>
        /// Phương thức mới để lấy chi tiết công việc
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Job> GetJobWithDetailsAsync(int id)
        {
            try
            {
                return await _context.Jobs
                    .Include(j => j.Company)
                    .Include(j => j.Applications)
                    .FirstOrDefaultAsync(j => j.Id == id);
                // Lưu ý: Không lọc job hết hạn ở đây để vẫn có thể xem chi tiết job đã hết hạn
                // Việc kiểm tra và hiển thị cảnh báo sẽ được thực hiện ở tầng Controller và View
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job details for ID {id}");
                return null;
            }
        }

        /// <summary>
        /// Phương thức để lấy công việc theo slug
        /// </summary>
        /// <param name="slug">Slug URL của công việc</param>
        /// <returns>Job với các thông tin chi tiết</returns>
        public async Task<Job> GetJobBySlugAsync(string slug)
        {
            try
            {
                return await _context.Jobs
                    .Include(j => j.Company)
                    .Include(j => j.Applications)
                    .FirstOrDefaultAsync(j => j.Slug == slug);
                // Lưu ý: Không lọc job hết hạn ở đây để vẫn có thể xem chi tiết job đã hết hạn
                // Việc kiểm tra và hiển thị cảnh báo sẽ được thực hiện ở tầng Controller và View
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job details for slug {slug}");
                return null;
            }
        }

        /// <summary>
        /// Phương thức mới để lấy công việc phân trang cơ bản
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="location"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<Job> Jobs, int TotalCount)> GetJobsPagedAsync(
        string keyword = "",
        string location = "",
        int page = 1,
        int pageSize = 12)
        {
            var query = _context.Jobs.AsQueryable();

            // Áp dụng bộ lọc
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(j => j.Location.Contains(location));
            }

            // Đếm tổng số kết quả
            int totalCount = await query.CountAsync();

            // Áp dụng phân trang và lấy dữ liệu
            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(j => j.Company)
                .ToListAsync();

            return (jobs, totalCount);
        }
        public async Task<IEnumerable<Job>> GetSavedJobsByUserAsync(int userId)
        {
            return await _context.Jobs
                .Where(j => j.SavedByUsers.Any(s => s.UserId == userId))
                .Include(j => j.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetRecentJobsAsync(int count)
        {
            return await _context.Jobs
                .Where(j => j.IsActive && j.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(j => j.CreatedAt)
                .Take(count)
                .Include(j => j.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetFeaturedJobsAsync(int count)
        {
            return await _context.Jobs
                .Where(j => j.IsActive && j.ExpiryDate > DateTime.UtcNow)
                // Trong thực tế, có thể thêm tiêu chí khác để xác định job nổi bật
                // Ví dụ: có nhiều lượt xem, được đánh dấu là featured, lương cao, v.v.
                .OrderByDescending(j => j.SalaryMax)
                .ThenByDescending(j => j.CreatedAt)
                .Take(count)
                .Include(j => j.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsByAdvancedFilterAsync(
            string keyword,
            string location,
            int? categoryId,
            JobType? jobType,
            ExperienceLevel? experienceLevel,
            decimal? minSalary,
            decimal? maxSalary,
            string jobLevel = null,
            DateTime? postedAfter = null,
            int? page = null,
            int? pageSize = null)
        {
            var query = _context.Jobs.AsQueryable();

            // Apply basic filters
            query = ApplyFilters(query, keyword, location, categoryId, jobType, experienceLevel, minSalary, maxSalary);

            // Filter by job level if provided
            if (!string.IsNullOrEmpty(jobLevel))
            {
                // Assuming job level corresponds to experience level enum values
                if (Enum.TryParse<ExperienceLevel>(jobLevel.Replace(" ", ""), out var jobLevelEnum))
                {
                    query = query.Where(j => j.ExperienceLevel == jobLevelEnum);
                }
            }

            // Filter by posting date
            if (postedAfter.HasValue)
            {
                query = query.Where(j => j.CreatedAt >= postedAfter.Value);
            }

            // Add sorting by newest jobs first
            query = query.OrderByDescending(j => j.CreatedAt);

            // Apply pagination if specified
            if (page.HasValue && pageSize.HasValue)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query
                .Include(j => j.Company)
                .Include(j => j.Categories)
                    .ThenInclude(jc => jc.Category)
                .ToListAsync();
        }
        // Phân trang và lọc theo các tiêu chí
        public async Task<(IEnumerable<Job> Jobs, int TotalCount)> GetJobsAdvancedPagedAsync(
            string keyword = "",
            string location = "",
            int? categoryId = null,
            JobType? jobType = null,
            ExperienceLevel? experienceLevel = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            ExperienceLevel? jobLevelEnum = null,
            DateTime? postedAfter = null,
            int page = 1,
            int pageSize = 12)
        {
            var query = _context.Jobs.AsQueryable();

            // Apply all filters
            query = ApplyFilters(query, keyword, location, categoryId, jobType, experienceLevel, minSalary, maxSalary);

            // Filter by job level if provided
            if (jobLevelEnum.HasValue)
            {
                // if (Enum.TryParse<ExperienceLevel>(jobLevel.Replace(" ", ""), out var jobLevelEnum))
                // {
                //     query = query.Where(j => j.ExperienceLevel == jobLevelEnum);
                // }
                query = query.Where(j => j.ExperienceLevel == jobLevelEnum.Value);
            }

            // Filter by posting date
            if (postedAfter.HasValue)
            {
                DateTime utcPostedAfter = DateTime.SpecifyKind(postedAfter.Value, DateTimeKind.Utc);
                query = query.Where(j => j.CreatedAt >= utcPostedAfter);
            }

            // Đếm tổng số kết quả
            int totalCount = await query.CountAsync();

            // Add sorting by newest jobs first
            query = query.OrderByDescending(j => j.CreatedAt);

            // Áp dụng phân trang và lấy dữ liệu
            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(j => j.Company)
                .Include(j => j.Categories)
                    .ThenInclude(jc => jc.Category)
                .ToListAsync();

            return (jobs, totalCount);
        }

        public async Task<IEnumerable<Job>> GetRelatedJobsAsync(int companyId)
        {
            return await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.CompanyId == companyId && j.IsActive && j.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        // Kiểm tra xem người dùng đã ứng tuyển vào công việc này chưa
        // jobId: ID của công việc cần kiểm tra
        // userId: ID của người dùng cần kiểm tra
        // Trả về true nếu người dùng đã ứng tuyển, false nếu chưa
        public async Task<bool> HasUserAppliedToJobAsync(int jobId, int userId)
        {
            return await _context.JobApplications
                .AnyAsync(j => j.JobId == jobId && j.ApplicantId == userId);
        }

        public async Task<JobApplication> AddJobApplicationAsync(JobApplication jobApplication)
        {
            await _context.JobApplications.AddAsync(jobApplication);
            await _context.SaveChangesAsync();
            return jobApplication;
        }

        public async Task<int> GetActiveJobCountByCompanyIdAsync(int companyId)
        {
            return await _context.Jobs
                .CountAsync(j => j.CompanyId == companyId && j.IsActive && j.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<List<Job>> GetJobsByCompanyIdAsync(int companyId)
        {
            return await _context.Jobs
                .Where(j => j.CompanyId == companyId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsByCompanyIdAsync(int companyId, int limit)
        {
            return await _context.Jobs
                .Where(j => j.CompanyId == companyId)
                .OrderByDescending(j => j.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<(List<Job> Jobs, int TotalCount)> GetAllJobsByCompanyIdAsync(int companyId, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Jobs
                    .AsNoTracking()
                    .Where(j => j.CompanyId == companyId)
                    .OrderByDescending(j => j.CreatedAt);

                var totalCount = await query.CountAsync();

                var jobs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(j => new Job
                    {
                        Id = j.Id,
                        Title = j.Title,
                        JobType = j.JobType,
                        Description = j.Description,
                        Requirements = j.Requirements,
                        Benefits = j.Benefits,
                        Location = j.Location,
                        SalaryMin = j.SalaryMin,
                        SalaryMax = j.SalaryMax,
                        CreatedAt = j.CreatedAt,
                        ExpiryDate = j.ExpiryDate,
                        IsActive = j.IsActive,
                        Metadata = j.Metadata,
                        CompanyId = j.CompanyId,
                        Applications = j.Applications.Select(a => new JobApplication
                        {
                            Id = a.Id,
                            Status = a.Status,
                            AppliedDate = a.AppliedDate,
                            ApplicantId = a.ApplicantId,
                            Applicant = new ApplicationUser
                            {
                                Id = a.Applicant.Id,
                                FirstName = a.Applicant.FirstName,
                                LastName = a.Applicant.LastName
                            }
                        }).ToList()
                    })
                    .ToListAsync();

                return (jobs, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs for company {CompanyId}", companyId);
                throw;
            }
        }

        // Get company by ID
        public async Task<Company> GetCompanyByIdAsync(int companyId)
        {
            return await _context.Companies.FindAsync(companyId);
        }

        // Helper method to apply filters
        private IQueryable<Job> ApplyFilters(
            IQueryable<Job> query,
            string keyword,
            string location,
            int? categoryId,
            JobType? jobType,
            ExperienceLevel? experienceLevel,
            decimal? minSalary,
            decimal? maxSalary)
        {
            // Filter by active jobs
            query = query.Where(j => j.IsActive && j.ExpiryDate > DateTime.UtcNow);

            // Filter by keyword with improved search
            if (!string.IsNullOrEmpty(keyword))
            {
                string[] keywords = keyword.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Use ToList() to perform the keyword filtering in memory instead of trying to translate to SQL
                if (keywords.Length > 0)
                {
                    // Perform initial database query
                    var baseQuery = query;

                    // Execute first query and convert to in-memory collection
                    var jobs = baseQuery.Include(j => j.Company).ToList();

                    // Perform the keyword filtering in memory
                    var filteredJobs = jobs.Where(j =>
                        keywords.Any(k =>
                            j.Title.ToLower().Contains(k) ||
                            j.Description.ToLower().Contains(k) ||
                            j.Requirements.ToLower().Contains(k) ||
                            j.Company.Name.ToLower().Contains(k)
                        )
                    ).ToList();

                    // Get the IDs of the filtered jobs
                    var filteredJobIds = filteredJobs.Select(j => j.Id).ToList();

                    // Update the query to only include these jobs
                    query = query.Where(j => filteredJobIds.Contains(j.Id));
                }
            }

            // Filter by location
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(j => EF.Functions.Like(j.Location, "%" + location + "%"));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(j => j.Categories.Any(c => c.CategoryId == categoryId.Value));
            }

            // Filter by job type
            if (jobType.HasValue)
            {
                string jobTypeString = ((int)jobType.Value).ToString();
                query = query.Where(j => EF.Functions.Like(j.JobType.ToString(), jobTypeString));
            }

            // Filter by experience level
            if (experienceLevel.HasValue)
            {
                string experienceLevelString = ((int)experienceLevel.Value).ToString();
                query = query.Where(j => EF.Functions.Like(j.ExperienceLevel.ToString(), experienceLevelString));
            }

            // Filter by salary range
            if (minSalary.HasValue)
            {
                query = query.Where(j => j.SalaryMax >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(j => j.SalaryMin <= maxSalary.Value);
            }

            return query;
        }

        public async Task<IEnumerable<Job>> GetRecentJobsByCompanyIdAsync(int companyId, int count)
        {
            return await _context.Jobs
                .Where(j => j.CompanyId == companyId)
                .OrderByDescending(j => j.CreatedAt)
                .Take(count)
                .Include(j => j.Applications)
                .ToListAsync();
        }

        public async Task<int> GetTotalJobsByCompanyIdAsync(int companyId)
        {
            return await _context.Jobs
                .CountAsync(j => j.CompanyId == companyId);
        }

        public async Task<int> GetTotalApplicationsByCompanyIdAsync(int companyId)
        {
            return await _context.JobApplications
                .CountAsync(ja => ja.Job.CompanyId == companyId);
        }

        public async Task<Job> GetJobByApplicationIdAsync(int applicationId)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Include(j => j.Applications)
                    .ThenInclude(a => a.Applicant)
                .FirstOrDefaultAsync(j => j.Applications.Any(a => a.Id == applicationId));
        }

        public async Task DeleteApplicationAsync(int applicationId)
        {
            var application = await _context.Set<JobApplication>().FindAsync(applicationId);
            if (application != null)
            {
                _context.Set<JobApplication>().Remove(application);
            }
        }

        public async Task UpdateApplicationStatusAsync(int applicationId, int status)
        {
            var application = await _context.Set<JobApplication>().FindAsync(applicationId);
            if (application != null)
            {
                application.Status = (ApplicationStatus)status;
                application.UpdatedAt = DateTime.UtcNow;

                _context.Update(application);
            }
        }

        public async Task<Job> GetJobWithApplicationsAsync(int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Applications)
                        .ThenInclude(a => a.Applicant)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job != null)
                {
                    _logger.LogInformation($"Found job {job.Id} with {job.Applications?.Count ?? 0} applications");
                }
                else
                {
                    _logger.LogWarning($"Job with ID {jobId} not found");
                }

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job with applications for job ID {jobId}");
                throw;
            }
        }

        public async Task<List<JobApplication>> GetJobApplicationsByUserIdAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.JobApplications
                    .Where(a => a.ApplicantId == userId)
                    .Include(a => a.Job)
                        .ThenInclude(j => j.Company)
                    .OrderByDescending(a => a.AppliedDate);

                var totalCount = await query.CountAsync();

                var applications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return applications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job applications for user ID {userId}");
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin lần apply gần nhất của user cho một job cụ thể
        /// </summary>
        /// <param name="jobId">ID của công việc</param>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Thông tin lần apply gần nhất hoặc null nếu chưa từng apply</returns>
        public async Task<JobApplication?> GetUserLatestApplicationAsync(int jobId, int userId)
        {
            try
            {
                return await _context.JobApplications
                    .Where(a => a.JobId == jobId && a.ApplicantId == userId)
                    .OrderByDescending(a => a.AppliedDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving latest job application for user ID {userId} and job ID {jobId}");
                return null;
            }
        }

        /// <summary>
        /// Cập nhật thông tin JobApplication
        /// </summary>
        /// <param name="application">JobApplication cần cập nhật</param>
        /// <returns>Task</returns>
        public async Task UpdateJobApplicationAsync(JobApplication application)
        {
            try
            {
                _context.JobApplications.Update(application);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating job application: {application.Id}");
                throw;
            }
        }
    }
}