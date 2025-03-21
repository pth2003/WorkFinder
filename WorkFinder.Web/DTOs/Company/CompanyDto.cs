using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkFinder.Web.DTOs.Company
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string Website { get; set; }
        public string Location { get; set; }
        public int EmployeeCount { get; set; }
        public string Industry { get; set; }
        public bool IsVerified { get; set; }

        public int OpenJobsCount { get; set; }
        public DateTime FoundedDate { get; set; }


    }
}