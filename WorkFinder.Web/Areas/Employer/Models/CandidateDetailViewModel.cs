using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class CandidateDetailViewModel
    {
        // Thông tin từ ApplicationUser
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;

        // Thông tin từ Resume
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Certifications { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string ResumeFileUrl { get; set; } = string.Empty;

        // Thông tin từ JobApplication
        public int ApplicationId { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }

        // Thông tin bổ sung
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
    }
}