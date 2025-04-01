using System.Collections.Generic;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class JobApplicationsViewModel
    {
        public Job Job { get; set; }
        public IEnumerable<JobApplication> Applications { get; set; }
        public Resume Resume { get; set; }

        // Thêm Dictionary để map Resume với mỗi User ID
        public Dictionary<int, Resume> UserResumes { get; set; } = new Dictionary<int, Resume>();
    }
}