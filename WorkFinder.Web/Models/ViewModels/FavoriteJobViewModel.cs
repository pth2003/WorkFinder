using System;

namespace WorkFinder.Web.Models.ViewModels
{
    public class FavoriteJobViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string JobType { get; set; }
        public string Location { get; set; }
        public string Salary { get; set; }
        public string CompanyName { get; set; }
        public DateTime DateAdded { get; set; }
        public string? CompanyLogo { get; internal set; }
    }
}