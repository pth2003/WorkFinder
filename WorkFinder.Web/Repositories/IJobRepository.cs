using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;


namespace WorkFinder.Web.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        Task<Job> GetByIdAsync(int id);
        // Task<List<Job>> GetActiveJobsAsync();
        Task<List<Job>> GetJobsByCompanyIdAsync(int companyId);
        Task<(List<Job> Jobs, int TotalCount)> GetAllJobsByCompanyIdAsync(int companyId, int page = 1, int pageSize = 10);
        Task<Company> GetCompanyByIdAsync(int companyId);

        // Các phương thức bổ sung đặc thù cho Job
        Task<IEnumerable<Job>> GetActiveJobsAsync();
        /// <summary>
        /// Phương thức mới để lấy công việc theo công ty
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        Task<IEnumerable<Job>> GetJobsByCompanyAsync(int companyId);
        /// <summary>
        /// Phương thức mới để lấy công việc theo danh mục
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        Task<IEnumerable<Job>> GetJobsByCategoryAsync(int categoryId);
        /// <summary>
        /// Phương thức mới để lấy công việc theo các tiêu chí cơ bản 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="location"></param>
        /// <param name="categoryId"></param>
        /// <param name="jobType"></param>
        Task<IEnumerable<Job>> GetJobsByFilterAsync(string keyword, string location, int? categoryId, JobType? jobType, ExperienceLevel? experienceLevel, decimal? minSalary, decimal? maxSalary);
        /// <summary>
        /// Phương thức mới để lấy chi tiết công việc
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Job> GetJobWithDetailsAsync(int id);
        /// <summary>
        /// Phương thức mới để lấy công việc theo slug
        /// </summary>
        /// <param name="slug">Slug URL của công việc</param>
        /// <returns>Job với các thông tin chi tiết</returns>
        Task<Job> GetJobBySlugAsync(string slug);
        /// <summary>
        /// Phương thức mới để lấy công việc phân trang cơ bản
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="location"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(IEnumerable<Job> Jobs, int TotalCount)> GetJobsPagedAsync(
         string keyword = "",
         string location = "",
         int page = 1,
         int pageSize = 12);
        Task<IEnumerable<Job>> GetSavedJobsByUserAsync(int userId);
        /// <summary>
        /// Phương thức mới để lấy công việc mới nhất
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<IEnumerable<Job>> GetRecentJobsAsync(int count);

        /// <summary>
        /// Phương thức mới để lấy công việc nổi bật
        /// </summary>
        /// <param name="count">Số lượng công việc muốn lấy</param>
        /// <returns>Danh sách công việc nổi bật</returns>
        Task<IEnumerable<Job>> GetFeaturedJobsAsync(int count);

        /// <summary>
        /// Phương thức mới để lấy công việc theo các tiêu chí nâng cao
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="location"></param>
        /// <param name="categoryId"></param>
        /// <param name="jobType"></param>
        /// <param name="experienceLevel"></param>
        /// <param name="minSalary"></param>
        /// <param name="maxSalary"></param>
        /// <param name="jobLevel"></param>
        /// <param name="postedAfter"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
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

        // Phương thức mới để lấy công việc phân trang
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

        // Phương thức mới để lấy công việc liên quan
        Task<IEnumerable<Job>> GetRelatedJobsAsync(int companyId);
        // Phương thức mới để kiểm tra xem user đã apply job này chưa
        Task<bool> HasUserAppliedToJobAsync(int jobId, int userId);
        // Phương thức mới để thêm ứng viên vào công việc
        Task<JobApplication> AddJobApplicationAsync(JobApplication jobApplication);

        // Phương thức mới để lấy số lượng công việc đang hoạt động của một công ty
        Task<int> GetActiveJobCountByCompanyIdAsync(int companyId);

        // Phương thức mới để lấy công việc gần đây của một công ty 
        Task<IEnumerable<Job>> GetRecentJobsByCompanyIdAsync(int companyId, int count);
        // Phương thức mới để lấy tổng số công việc của một công ty
        Task<int> GetTotalJobsByCompanyIdAsync(int companyId);
        // Phương thức mới để lấy tổng số ứng viên của một công ty
        Task<int> GetTotalApplicationsByCompanyIdAsync(int companyId);

        Task<IEnumerable<Job>> GetJobsByCompanyIdAsync(int companyId, int limit);

        Task<Job> GetJobByApplicationIdAsync(int applicationId);
        Task DeleteApplicationAsync(int applicationId);
        Task UpdateApplicationStatusAsync(int applicationId, int status);

        // Phương thức mới để lấy job cùng với tất cả applications của nó
        Task<Job> GetJobWithApplicationsAsync(int jobId);

        /// <summary>
        /// Phương thức mới để lấy tất cả job applications của một user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<List<JobApplication>> GetJobApplicationsByUserIdAsync(int userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Lấy thông tin lần apply gần nhất của user cho một job cụ thể
        /// </summary>
        /// <param name="jobId">ID của công việc</param>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Thông tin lần apply gần nhất hoặc null nếu chưa từng apply</returns>
        Task<JobApplication?> GetUserLatestApplicationAsync(int jobId, int userId);

        /// <summary>
        /// Cập nhật thông tin JobApplication
        /// </summary>
        /// <param name="application">JobApplication cần cập nhật</param>
        /// <returns>Task</returns>
        Task UpdateJobApplicationAsync(JobApplication application);
    }
}