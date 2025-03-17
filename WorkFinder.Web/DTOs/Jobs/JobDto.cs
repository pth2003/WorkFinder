using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkFinder.Web.DTOs.Jobs
{
    public class JobDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Benefits { get; set; }
        public string Location { get; set; }
        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }
        public string JobTypeName { get; set; } // Thay vì enum, dùng string đã convert
        public string ExperienceLevelName { get; set; } // Thay vì enum, dùng string đã convert
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }

        // Thông tin Company chỉ lấy những gì cần thiết
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }

        // Có thể bổ sung các trường phái sinh
        public string SalaryRange => $"{SalaryMin:C0}-{SalaryMax:C0}";
        public bool IsExpired => DateTime.Now > ExpiryDate;
    }
}