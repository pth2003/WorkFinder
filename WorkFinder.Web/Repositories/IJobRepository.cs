using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace WorkFinder.Web.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        // Các phương thức bổ sung đặc thù cho Job
        Task<IEnumerable<Job>> GetActiveJobsAsync();
        Task<IEnumerable<Job>> GetJobsByCompanyAsync(int companyId);
        Task<IEnumerable<Job>> GetJobsByCategoryAsync(int categoryId);
        Task<IEnumerable<Job>> GetJobsByFilterAsync(string keyword, string location, int? categoryId, JobType? jobType, ExperienceLevel? experienceLevel, decimal? minSalary, decimal? maxSalary);
        Task<Job> GetJobWithDetailsAsync(int id);
        Task<(IEnumerable<Job> Jobs, int TotalCount)> GetJobsPagedAsync(
         string keyword = "",
         string location = "",
         int page = 1,
         int pageSize = 12);
        Task<IEnumerable<Job>> GetSavedJobsByUserAsync(int userId);
        Task<IEnumerable<Job>> GetRecentJobsAsync(int count);

        Task<IEnumerable<Job>> GetJobsByAdvancedFilterAsync(
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
       int? pageSize = null);

        Task<(IEnumerable<Job> Jobs, int TotalCount)> GetJobsAdvancedPagedAsync(
            string keyword = "",
            string location = "",
            int? categoryId = null,
            JobType? jobType = null,
            ExperienceLevel? experienceLevel = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            ExperienceLevel? jobLevel = null,
            DateTime? postedAfter = null,
            int page = 1,
            int pageSize = 12);
    }
}