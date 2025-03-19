using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public class ResumeRepository : Repository<Resume>, IResumeRepository
    {
        private readonly WorkFinderContext _context;

        public ResumeRepository(WorkFinderContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Resume>> GetResumesByUserIdAsync(int userId)
        {
            return await _context.Resumes
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<Resume> GetResumeByUserIdAsync(int userId)
        {
            return await _context.Resumes.FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task AddResumeAsync(Resume resume)
        {
            await _context.Resumes.AddAsync(resume);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserHasResumeAsync(int userId)
        {
            return await _context.Resumes.AnyAsync(r => r.UserId == userId);
        }
    }
}