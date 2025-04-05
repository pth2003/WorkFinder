using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public interface ISaveJobRepository : IRepository<SavedJob>
    {
        Task<IEnumerable<SavedJob>> GetSavedJobsByUserIdAsync(int userId);
        Task<SavedJob> GetSavedJobByIdAsync(int id);
        Task<bool> CreateSavedJobAsync(SavedJob savedJob);
        Task<bool> UpdateSavedJobAsync(SavedJob savedJob);
        Task<bool> DeleteSavedJobAsync(int id);
    }
}