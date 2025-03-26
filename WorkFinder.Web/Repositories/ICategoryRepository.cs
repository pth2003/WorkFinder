using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesByJobId(int jobId);

        // Thêm các phương thức mới cần thiết
        Task<Category> GetCategoryByNameAsync(string name);
        Task AddJobCategoryAsync(int jobId, int categoryId);
    }
}