using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class CompanySetupOrganizationViewModel
    {
        [Required(ErrorMessage = "Organization type is required")]
        [Display(Name = "Organization Type")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Industry is required")]
        [Display(Name = "Industry")]
        public string Industry { get; set; }

        [Display(Name = "Number of Employees")]
        public int EmployeeCount { get; set; }

        [Display(Name = "Founded Date")]
        [DataType(DataType.Date)]
        public DateTime? FoundedDate { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Company Website")]
        public string Website { get; set; }

        [Display(Name = "Company Vision")]
        public string Vision { get; set; }
    }
}