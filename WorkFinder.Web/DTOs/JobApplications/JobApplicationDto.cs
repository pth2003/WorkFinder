using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WorkFinder.Web.DTOs.JobApplications
{
    public class JobApplicationDto
    {
        public int JobId { get; set; }

        [Required(ErrorMessage = "Cover letter is required")]
        [MinLength(50, ErrorMessage = "Cover letter must be at least 50 characters")]
        public string CoverLetter { get; set; }

        // Cho phép chọn sử dụng resume đã có hoặc upload mới
        public bool UseExistingResume { get; set; }

        // File là nullable, sẽ kiểm tra trong controller tùy theo UseExistingResume
        public IFormFile? ResumeFile { get; set; }
    }
}