using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace WorkFinder.Web.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly WorkFinderContext _context;
    private readonly DbSet<T> _dbSet;
    public Repository(WorkFinderContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
            _dbSet.Remove(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}