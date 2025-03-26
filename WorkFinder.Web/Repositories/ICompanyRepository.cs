using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<IEnumerable<Company>> GetAllAsync();

        Task<IEnumerable<Company>> GetVerifiedCompaniesAsync();

        // Thêm phương thức phân trang với lọc và tìm kiếm
        Task<(IEnumerable<Company> companies, int totalCount)> GetCompaniesPagedAsync(
            string keyword = "",
            string industry = "",
            string location = "",
            bool? isVerified = null,
            int page = 1,
            int pageSize = 10);

        // Phương thức lấy số lượng công việc đang có của một công ty
        Task<int> GetActiveJobCountByCompanyIdAsync(int companyId);

        // Phương thức lấy các ngành phổ biến
        Task<List<string>> GetPopularIndustriesAsync(int count);

        // Phương thức lấy các địa điểm phổ biến
        Task<List<string>> GetPopularLocationsAsync(int count);

        Task<IEnumerable<Company>> GetCompaniesByIndustryAsync(string industry, int limit, int excludeCompanyId = 0);

        Task<IEnumerable<Company>> GetTopCompaniesAsync(int limit);
        Task<Company> GetByOwnerIdAsync(int ownerId);

        // Task<int> GetTotalJobsByCompanyIdAsync(int companyId);
    }
}