using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WorkFinder.Web.Data;
using WorkFinder.Web.DTOs.Candidate;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly WorkFinderContext _context;


        public CandidateRepository(WorkFinderContext context)
        {
            _context = context;
        }

        public bool SaveJob(int userId, int jobId)
        {
            SavedJob savedJob = new SavedJob()
            {
                SavedDate = DateTime.UtcNow,
                JobId = jobId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                // User = new ApplicationUser(),
                // Job = new Job()
            };

            _context.SavedJobs.Add(savedJob);
            int row = _context.SaveChanges();

            return row > 0 ? true : false;
        }

        public bool DeleteSavedJob(int userId, int jobId)
        {
            SavedJob savedJob = (from s in _context.SavedJobs where s.JobId == jobId && s.UserId == userId select s).FirstOrDefault();

            if (savedJob != null)
            {
                _context.SavedJobs.Remove(savedJob);
                int row = _context.SaveChanges();
                return row > 0 ? true : false;
            }
            return false;
        }

        public bool CheckSavedJob(int userId, int jobId)
        {
            SavedJob savedJob = (from s in _context.SavedJobs where s.JobId == jobId && s.UserId == userId select s).FirstOrDefault();
            return savedJob != null ? true : false;
        }

        public int CountSavedJob(int userId)
        {
            List<SavedJob> savedJobList = (from s in _context.SavedJobs where s.UserId == userId select s).ToList();
            return savedJobList.Count();
        }

        public int CountAppliedJob(int userId)
        {
            List<JobApplication> jobApplicationList = (from j in _context.JobApplications where j.ApplicantId == userId select j).ToList();
            return jobApplicationList.Count();
        }

        public List<Job> GetSavedJobList(int userId)
        {
            List<SavedJob> savedJobList = (from s in _context.SavedJobs where s.UserId == userId select s).ToList();
            List<Job> jobList = new List<Job>();
            foreach (SavedJob savedJob in savedJobList)
            {
                Job job = (from j in _context.Jobs where j.Id == savedJob.JobId select j).FirstOrDefault();
                jobList.Add(job);
            }
            return jobList;
        }

        public List<JobApplication> GetAppliedJobList(int userId)
        {
            List<JobApplication> jobApplicationList = (from j in _context.JobApplications where j.ApplicantId == userId select j).ToList();
            return jobApplicationList;
        }

        public ApplicationUser GetUserByEmail(string email)
        {
            ApplicationUser applicationUser = (from a in _context.Users where a.Email == email select a).FirstOrDefault();
            return applicationUser;
        }

        public ApplicationUser UpdateUser(UserSettingDto userSettingDto)
        {
            ApplicationUser applicationUser = (from a in _context.Users where a.Email == userSettingDto.Email select a).FirstOrDefault();
            if (applicationUser != null)
            {
                applicationUser.FirstName = userSettingDto.FirstName;
                applicationUser.LastName = userSettingDto.LastName;
                applicationUser.PhoneNumber = userSettingDto.PhoneNumber;
                // applicationUser.
                if (userSettingDto.ProfilePicture != "null")
                {
                    if (!string.IsNullOrEmpty(applicationUser.ProfilePicture))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", applicationUser.ProfilePicture.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    applicationUser.ProfilePicture = userSettingDto.ProfilePicture;
                }
                else
                {
                    applicationUser.ProfilePicture = "";
                }
                int rows = _context.SaveChanges();
                return rows > 0 ? applicationUser : null;
            }
            return null;
        }

        public Resume GetResumeByUserId(int id)
        {
            Resume resume = (from r in _context.Resumes where r.UserId == id select r).FirstOrDefault();
            return resume;
        }

        public Resume UpdateResume(ResumeSettingDto resumeSettingDto, int userId)
        {
            Resume resume = (from r in _context.Resumes where r.UserId == userId select r).FirstOrDefault();
            if (resume == null)
            {
                resume = new Resume()
                {
                    Summary = resumeSettingDto.Summary,
                    Skills = resumeSettingDto.Skills,
                    Education = resumeSettingDto.Education,
                    Experience = resumeSettingDto.Experience,
                    Certifications = resumeSettingDto.Certifications,
                    Languages = resumeSettingDto.Languages,
                };
                if (resumeSettingDto.FileUrl != "null")
                {
                    resume.FileUrl = resumeSettingDto.FileUrl;

                }
                else
                {
                    resume.FileUrl = "";
                }
                resume.UserId = userId;
                resume.CreatedAt = DateTime.UtcNow;
                _context.Resumes.Add(resume);
                int rows = _context.SaveChanges();
                return rows > 0 ? resume : null;
            }
            else
            {
                resume.Summary = resumeSettingDto.Summary;
                resume.Skills = resumeSettingDto.Skills;
                resume.Education = resumeSettingDto.Education;
                resume.Experience = resumeSettingDto.Experience;
                resume.Certifications = resumeSettingDto.Certifications;
                resume.Languages = resumeSettingDto.Languages;

                if (resumeSettingDto.FileUrl != "null")
                {
                    if (!string.IsNullOrEmpty(resume.FileUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resume.FileUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    resume.FileUrl = resumeSettingDto.FileUrl;
                }
                resume.UpdatedAt = DateTime.UtcNow;
                int rows = _context.SaveChanges();
                return rows > 0 ? resume : null;
            }
            return null;
        }
    }
}