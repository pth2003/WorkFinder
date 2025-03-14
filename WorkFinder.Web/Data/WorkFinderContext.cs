using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Data;

public class WorkFinderContext : IdentityDbContext<ApplicationUser , IdentityRole<int>, int>
{
    public WorkFinderContext(DbContextOptions<WorkFinderContext> options) : base(options)
    {
        
    }
    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<SavedJob> SavedJobs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<JobCategory> JobCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");
        
        modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.ProfilePicture).HasMaxLength(255);
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(255);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Industry).HasMaxLength(50);
                entity.Property(e => e.Logo).HasMaxLength(255);
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("jobs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Requirements).HasColumnType("text");
                entity.Property(e => e.Benefits).HasColumnType("text");
                entity.Property(e => e.SalaryMin).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SalaryMax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.JobType).HasConversion<string>();
                entity.Property(e => e.ExperienceLevel).HasConversion<string>();
            });

            modelBuilder.Entity<Resume>(entity =>
            {
                entity.ToTable("resumes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Summary).HasColumnType("text");
                entity.Property(e => e.Skills).HasColumnType("text");
                entity.Property(e => e.Education).HasColumnType("text");
                entity.Property(e => e.Experience).HasColumnType("text");
                entity.Property(e => e.Certifications).HasColumnType("text");
                entity.Property(e => e.Languages).HasColumnType("text");
                entity.Property(e => e.FileUrl).HasMaxLength(255);
            });

            modelBuilder.Entity<JobApplication>(entity =>
            {
                entity.ToTable("job_applications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CoverLetter).HasColumnType("text");
                entity.Property(e => e.Status).HasConversion<string>();
            });

            modelBuilder.Entity<SavedJob>(entity =>
            {
                entity.ToTable("saved_jobs");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
            });

            modelBuilder.Entity<JobCategory>(entity =>
            {
                entity.ToTable("job_categories");
                entity.HasKey(e => new { e.JobId, e.CategoryId });
            });

            // Cấu hình mối quan hệ
            
            // User - Resume (1-1)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Resume)
                .WithOne(r => r.User)
                .HasForeignKey<Resume>(r => r.UserId);

            // User - Company (1-1)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithOne(c => c.Owner)
                .HasForeignKey<Company>(c => c.OwnerId);

            // Company - Job (1-n)
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Jobs)
                .WithOne(j => j.Company)
                .HasForeignKey(j => j.CompanyId);

            // Job - JobApplication (1-n)
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Applications)
                .WithOne(a => a.Job)
                .HasForeignKey(a => a.JobId);

            // User - JobApplication (1-n)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.JobApplications)
                .WithOne(a => a.Applicant)
                .HasForeignKey(a => a.ApplicantId);

            // User - SavedJob (1-n)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.SavedJobs)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId);

            // Job - SavedJob (1-n)
            modelBuilder.Entity<Job>()
                .HasMany(j => j.SavedByUsers)
                .WithOne(s => s.Job)
                .HasForeignKey(s => s.JobId);

            // Job - Category (n-n) thông qua JobCategory
            modelBuilder.Entity<JobCategory>()
                .HasOne(jc => jc.Job)
                .WithMany(j => j.Categories)
                .HasForeignKey(jc => jc.JobId);

            modelBuilder.Entity<JobCategory>()
                .HasOne(jc => jc.Category)
                .WithMany(c => c.Jobs)
                .HasForeignKey(jc => jc.CategoryId);
    }
}