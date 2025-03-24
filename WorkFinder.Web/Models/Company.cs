namespace WorkFinder.Web.Models;

public class Company : BaseEntity
{
    // Thông tin cơ bản (Bước 1)
    public string Name { get; set; }
    public string Description { get; set; }
    public string Logo { get; set; }
    public string Banner { get; set; }

    // Thông tin tổ chức (Bước 2)
    public int CategoryId { get; set; }  // OrganizationType
    public virtual Category Category { get; set; }
    public string Industry { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime? FoundedDate { get; set; }
    public string Website { get; set; }
    public string Vision { get; set; }

    // Thông tin social media (Bước 3)


    // Thông tin liên hệ (Bước 4)
    public string Location { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    // Các trường khác
    public bool IsVerified { get; set; }

    // Navigation properties
    public int OwnerId { get; set; }
    public virtual ApplicationUser Owner { get; set; } = null!;
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    public virtual ICollection<SocialLink> SocialLinks { get; set; } = new List<SocialLink>();
}