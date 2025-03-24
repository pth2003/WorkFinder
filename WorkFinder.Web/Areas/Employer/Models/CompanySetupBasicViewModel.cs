using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class CompanySetupBasicViewModel
    {
        [Required(ErrorMessage = "Company name is required")]
        [Display(Name = "Company Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Company Logo")]
        [JsonIgnore] // Không serialize khi lưu vào session
        public IFormFile Logo { get; set; }

        [Display(Name = "Company Banner")]
        [JsonIgnore] // Không serialize khi lưu vào session
        public IFormFile Banner { get; set; }

        // Lưu đường dẫn file đã upload
        public string LogoPath { get; set; }
        public string BannerPath { get; set; }
    }
}