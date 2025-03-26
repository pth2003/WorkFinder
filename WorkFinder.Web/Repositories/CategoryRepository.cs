using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly WorkFinderContext _context;

        public CategoryRepository(WorkFinderContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // public async Task<IEnumerable<Category>> GetActiveCategories()
        // {
        //     return await _context.Categories
        //         .Where(c => c.IsActive)
        //         .OrderBy(c => c.Name)
        //         .ToListAsync();
        // }

        public async Task<IEnumerable<Category>> GetCategoriesByJobId(int jobId)
        {
            return await _context.JobCategories
                .Where(jc => jc.JobId == jobId)
                .Select(jc => jc.Category)
                .ToListAsync();
        }
        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task AddJobCategoryAsync(int jobId, int categoryId)
        {
            var jobCategory = await _context.JobCategories.FirstOrDefaultAsync(jc => jc.JobId == jobId && jc.CategoryId == categoryId);
            if (jobCategory == null)
            {
                jobCategory = new JobCategory
                {
                    JobId = jobId,
                    CategoryId = categoryId
                };
                await _context.JobCategories.AddAsync(jobCategory);
                await _context.SaveChangesAsync();
            }
        }
    }
}