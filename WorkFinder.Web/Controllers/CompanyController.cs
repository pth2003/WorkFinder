using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.DTOs.Company;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers;
[Route("[controller]")]
public class CompanyController : Controller
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobRepository _jobRepository;

    public CompanyController(
        ICompanyRepository companyRepository,
        IJobRepository jobRepository)
    {
        _companyRepository = companyRepository;
        _jobRepository = jobRepository;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(
        string keyword = "",
        string industry = "",
        string location = "",
        bool? isVerified = null,
        int page = 1,
        int pageSize = 12,
        string sortBy = "Latest")
    {
        // Lấy danh sách công ty với phân trang và lọc
        var (companies, totalCount) = await _companyRepository.GetCompaniesPagedAsync(
            keyword, industry, location, isVerified, page, pageSize);

        // Chuyển đổi từ entity sang DTO
        var companyDtos = new List<CompanyDto>();
        foreach (var company in companies)
        {
            var openJobsCount = await _jobRepository.GetActiveJobCountByCompanyIdAsync(company.Id);

            companyDtos.Add(new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description?.Length > 100
                    ? company.Description.Substring(0, 97) + "..."
                    : company.Description,
                Logo = company.Logo,
                Website = company.Website,
                Location = company.Location,
                EmployeeCount = company.EmployeeCount,
                Industry = company.Industry,
                IsVerified = company.IsVerified,
                OpenJobsCount = openJobsCount,
                FoundedDate = company.CreatedAt // Sử dụng CreatedAt từ BaseEntity làm FoundedDate
            });
        }

        // Lấy danh sách ngành và địa điểm phổ biến
        var popularIndustries = await _companyRepository.GetPopularIndustriesAsync(10);
        var popularLocations = await _companyRepository.GetPopularLocationsAsync(10);

        // Tính toán thông tin phân trang
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Tạo và trả về ViewModel
        var viewModel = new CompanyIndexViewModel
        {
            Companies = companyDtos,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalCompanies = totalCount,
            SearchKeyword = keyword,
            IndustryFilter = industry,
            LocationFilter = location,
            IsVerifiedFilter = isVerified,
            PopularIndustries = popularIndustries,
            PopularLocations = popularLocations,
            SortBy = sortBy
        };

        return View(viewModel);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
            return NotFound();

        // Load company jobs
        var companyJobs = await _jobRepository.GetJobsByCompanyIdAsync(id, 5); // Get top 5 jobs
        ViewBag.CompanyJobs = companyJobs;

        // Get related companies (same industry)
        var relatedCompanies = await _companyRepository.GetCompaniesByIndustryAsync(company.Industry, 3, id);
        ViewBag.RelatedCompanies = relatedCompanies;

        return View(company);
    }


    [HttpGet]
    [Route("FilterCompanies")]
    public async Task<IActionResult> FilterCompanies(
        string keyword = "",
        string industry = "",
        string location = "",
        bool? isVerified = null,
        int page = 1,
        int pageSize = 12,
        string sortBy = "Latest",
        string viewMode = "grid")
    {
        // Lấy danh sách công ty với phân trang và lọc - giống với Index
        var (companies, totalCount) = await _companyRepository.GetCompaniesPagedAsync(
            keyword, industry, location, isVerified, page, pageSize);

        // Chuyển đổi từ entity sang DTO
        var companyDtos = new List<CompanyDto>();
        foreach (var company in companies)
        {
            var openJobsCount = await _jobRepository.GetActiveJobCountByCompanyIdAsync(company.Id);

            companyDtos.Add(new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description?.Length > 100
                    ? company.Description.Substring(0, 97) + "..."
                    : company.Description,
                Logo = company.Logo,
                Website = company.Website,
                Location = company.Location,
                EmployeeCount = company.EmployeeCount,
                Industry = company.Industry,
                IsVerified = company.IsVerified,
                OpenJobsCount = openJobsCount,
                FoundedDate = company.CreatedAt
            });
        }

        // Lấy danh sách ngành và địa điểm phổ biến
        var popularIndustries = await _companyRepository.GetPopularIndustriesAsync(10);
        var popularLocations = await _companyRepository.GetPopularLocationsAsync(10);

        // Tính toán thông tin phân trang
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Tạo ViewModel
        var viewModel = new CompanyIndexViewModel
        {
            Companies = companyDtos,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalCompanies = totalCount,
            SearchKeyword = keyword,
            IndustryFilter = industry,
            LocationFilter = location,
            IsVerifiedFilter = isVerified,
            PopularIndustries = popularIndustries,
            PopularLocations = popularLocations,
            SortBy = sortBy
        };

        // Thiết lập chế độ xem (grid hoặc list)
        ViewBag.ViewMode = viewMode;

        return PartialView("Partials/Company/_CompanyList", viewModel);
    }
}