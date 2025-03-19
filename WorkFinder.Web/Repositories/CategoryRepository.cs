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

    }
}