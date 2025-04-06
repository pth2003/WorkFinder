using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.DTOs.Candidate
{
    public class ResumeSettingDto
    {
        public string Summary { get; set; }
        public string Skills { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public string Certifications { get; set; }
        public string Languages { get; set; }
        public string FileUrl { get; set; }
        public IFormFile ImageFile { get; set; }

    }
}