using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Repositories
{
    public class JobRepository : Repository<Job>, IJobRepository
    {
        private readonly WorkFinderContext _context;

        public JobRepository(WorkFinderContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Job>> GetActiveJobsAsync()
        {
            return await _context.Jobs
                .Where(j => j.IsActive && j.ExpiryDate > DateTime.Now)
                .Include(j => j.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsByCompanyAsync(int companyId)
        {
            return await _context.Jobs
                .Where(j => j.CompanyId == companyId && j.IsActive)
                .Include(j => j.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsByCategoryAsync(int categoryId)
        {
            return await _context.Jobs
                .Where(j => j.Categories.Any(c => c.CategoryId == categoryId) && j.IsActive)
                .Include(j => j.Company)
                .ToListAsync();
        }

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

        public async Task<Job> GetJobWithDetailsAsync(int id)
        {
            return await _context.Jobs
                .Where(j => j.Id == id)
                .Include(j => j.Company)
                .Include(j => j.Categories)
                    .ThenInclude(jc => jc.Category)
                .FirstOrDefaultAsync();
        }

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
                .Where(j => j.CompanyId == companyId && j.IsActive)
                .Include(j => j.Company)
                .ToListAsync();
        }

        // Kiểm tra xem người dùng đã ứng tuyển vào công việc này chưa
        // jobId: ID của công việc cần kiểm tra
        // userId: ID của người dùng cần kiểm tra
        // Trả về true nếu người dùng đã ứng tuyển, false nếu chưa
        public async Task<bool> HasUserAppliedToJobAsync(int jobId, int userId)
        {
            return await _context.JobApplications
                .AnyAsync(a => a.JobId == jobId && a.ApplicantId == userId);
        }

        public async Task<JobApplication> AddJobApplicationAsync(JobApplication jobApplication)
        {
            await _context.JobApplications.AddAsync(jobApplication);
            await _context.SaveChangesAsync();
            return jobApplication;
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

            // Filter by keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                string pattern = $"{keyword}";

                query = query.Where(j =>
                    EF.Functions.Like(j.Title, pattern) ||
                    EF.Functions.Like(j.Description, pattern) ||
                    EF.Functions.Like(j.Requirements, pattern) ||
                    EF.Functions.Like(j.Company.Name, pattern));
            }

            // Filter by location
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(j => EF.Functions.Like(j.Location, $"%{location}%"));
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
    }
}