using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly WorkFinderContext _context;

        public CompanyRepository(WorkFinderContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Company>> GetVerifiedCompaniesAsync()
        {
            return await _context.Companies.Where(c => c.IsVerified).ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _context.Companies.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(IEnumerable<Company> companies, int totalCount)> GetCompaniesPagedAsync(
            string keyword = "",
            string industry = "",
            string location = "",
            bool? isVerified = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Companies.AsQueryable();

            // Áp dụng các điều kiện lọc
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.Name.Contains(keyword) ||
                                    (c.Description != null && c.Description.Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(industry))
            {
                query = query.Where(c => c.Industry == industry);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(c => c.Location.Contains(location));
            }

            if (isVerified.HasValue)
            {
                query = query.Where(c => c.IsVerified == isVerified.Value);
            }

            // Đếm tổng số kết quả
            int totalCount = await query.CountAsync();

            // Phân trang và sắp xếp
            var companies = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (companies, totalCount);
        }

        public async Task<int> GetActiveJobCountByCompanyIdAsync(int companyId)
        {
            return await _context.Jobs
                .Where(j => j.CompanyId == companyId && j.IsActive && j.ExpiryDate > DateTime.UtcNow)
                .CountAsync();
        }

        public async Task<List<string>> GetPopularIndustriesAsync(int count)
        {
            return await _context.Companies
                .Where(c => !string.IsNullOrEmpty(c.Industry))
                .GroupBy(c => c.Industry)
                .Select(g => new { Industry = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .Select(x => x.Industry)
                .ToListAsync();
        }

        public async Task<List<string>> GetPopularLocationsAsync(int count)
        {
            return await _context.Companies
                .Where(c => !string.IsNullOrEmpty(c.Location))
                .GroupBy(c => c.Location)
                .Select(g => new { Location = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .Select(x => x.Location)
                .ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetCompaniesByIndustryAsync(string industry, int limit, int excludeCompanyId = 0)
        {
            return await _context.Companies
                .Where(c => c.Industry == industry && c.Id != excludeCompanyId)
                .OrderByDescending(c => c.IsVerified)
                .ThenBy(c => Guid.NewGuid()) // Random order for variety
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetTopCompaniesAsync(int limit)
        {
            return await _context.Companies
                .OrderByDescending(c => c.IsVerified)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Company> GetByOwnerIdAsync(int ownerId)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.OwnerId == ownerId);
        }
    }
}