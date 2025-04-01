using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class CompanyProfileViewModel
    {
        public int CompanyId { get; set; }

        // Các thông tin từ các model hiện có
        public CompanySetupBasicViewModel BasicInfo { get; set; }
        public CompanySetupOrganizationViewModel OrganizationInfo { get; set; }
        public CompanySetupSocialViewModel SocialInfo { get; set; }
        public CompanySetupContactViewModel ContactInfo { get; set; }

        // Tab đang được chọn
        public string ActiveTab { get; set; }
    }
}