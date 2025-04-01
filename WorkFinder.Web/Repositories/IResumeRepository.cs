using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public interface IResumeRepository : IRepository<Resume>
    {
        Task<IEnumerable<Resume>> GetResumesByUserIdAsync(int userId);
        Task<Resume> GetResumeByUserIdAsync(int userId);
        Task AddResumeAsync(Resume resume);
        Task<bool> UserHasResumeAsync(int userId);
        Task<Dictionary<int, Resume>> GetResumesByUserIdsAsync(IEnumerable<int> userIds);
    }
}