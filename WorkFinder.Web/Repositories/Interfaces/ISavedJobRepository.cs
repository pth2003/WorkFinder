using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories.Interfaces
{
    public interface ISavedJobRepository
    {
        Task<SavedJob> SaveJobAsync(int jobId, int userId);
        Task<bool> RemoveSavedJobAsync(int jobId, int userId);
        Task<bool> IsJobSavedAsync(int jobId, int userId);
        Task<List<SavedJob>> GetSavedJobsByUserIdAsync(int userId);
    }
}