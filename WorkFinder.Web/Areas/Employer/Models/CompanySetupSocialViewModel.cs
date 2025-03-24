using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class SocialLinkViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Platform is required")]
        [Display(Name = "Platform")]
        public string Platform { get; set; }

        [Required(ErrorMessage = "URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "URL")]
        public string Url { get; set; }
    }

    public class CompanySetupSocialViewModel
    {
        public List<SocialLinkViewModel> SocialLinks { get; set; } = new List<SocialLinkViewModel>();
    }
}