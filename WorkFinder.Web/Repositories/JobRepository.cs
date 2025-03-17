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
            string keyword,
            string location,
            int? categoryId,
            JobType? jobType,
            ExperienceLevel? experienceLevel,
            decimal? minSalary,
            decimal? maxSalary,
            int page,
            int pageSize)
        {
            var query = _context.Jobs.AsQueryable();

            // Apply filters
            query = ApplyFilters(query, keyword, location, categoryId, jobType, experienceLevel, minSalary, maxSalary);

            // Count total results
            int totalCount = await query.CountAsync();

            // Apply pagination
            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(j => j.Company)
                .Include(j => j.Categories)
                    .ThenInclude(jc => jc.Category)
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
                query = query.Where(j =>
                    j.Title.Contains(keyword) ||
                    j.Description.Contains(keyword) ||
                    j.Requirements.Contains(keyword) ||
                    j.Company.Name.Contains(keyword));
            }

            // Filter by location
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(j => j.Location.Contains(location));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(j => j.Categories.Any(c => c.CategoryId == categoryId.Value));
            }

            // Filter by job type
            if (jobType.HasValue)
            {
                query = query.Where(j => j.JobType == jobType.Value);
            }

            // Filter by experience level
            if (experienceLevel.HasValue)
            {
                query = query.Where(j => j.ExperienceLevel == experienceLevel.Value);
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