using System;
using System.Collections.Generic;
using WorkFinder.Web.DTOs.Company;

namespace WorkFinder.Web.Models.ViewModels
{
    public class CompanyIndexViewModel
    {
        // Danh sách công ty
        public List<CompanyDto> Companies { get; set; } = new List<CompanyDto>();

        // Thông tin phân trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 12;
        public int TotalCompanies { get; set; }

        // Thông tin lọc
        public string SearchKeyword { get; set; }
        public string IndustryFilter { get; set; }
        public string LocationFilter { get; set; }
        public bool? IsVerifiedFilter { get; set; }

        // Danh sách các lựa chọn cho bộ lọc
        public List<string> PopularIndustries { get; set; } = new List<string>();
        public List<string> PopularLocations { get; set; } = new List<string>();

        // Các thuộc tính bổ sung
        public string SortBy { get; set; } = "Latest";
    }
}