using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public class SaveJobRepository : Repository<SavedJob>, ISaveJobRepository
    {
        private readonly WorkFinderContext _context;

        public SaveJobRepository(WorkFinderContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SavedJob>> GetSavedJobsByUserIdAsync(int userId)
        {
            return await _context.SavedJobs.Where(s => s.UserId == userId).ToListAsync();
        }
        public async Task<SavedJob> GetSavedJobByIdAsync(int id)
        {
            return await _context.SavedJobs.FindAsync(id);
        }

        public async Task<bool> CreateSavedJobAsync(SavedJob savedJob)
        {
            await _context.SavedJobs.AddAsync(savedJob);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateSavedJobAsync(SavedJob savedJob)
        {
            _context.SavedJobs.Update(savedJob);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteSavedJobAsync(int id)
        {
            var savedJob = await GetSavedJobByIdAsync(id);
            if (savedJob == null) return false;
            _context.SavedJobs.Remove(savedJob);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}