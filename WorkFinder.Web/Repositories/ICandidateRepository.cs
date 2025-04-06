using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.DTOs.Candidate;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Repositories
{
    public interface ICandidateRepository
    {
        public bool SaveJob(int userId, int jobId);
        public bool DeleteSavedJob(int userId, int jobId);
        public bool CheckSavedJob(int userId, int jobId);
        public int CountSavedJob(int userId);
        public int CountAppliedJob(int userId);
        public List<Job> GetSavedJobList(int userId);
        public List<JobApplication> GetAppliedJobList(int userId);
        public ApplicationUser GetUserByEmail(string email);
        public ApplicationUser UpdateUser(UserSettingDto userSettingDto);
        public Resume GetResumeByUserId(int id);
        public Resume UpdateResume(ResumeSettingDto resumeSettingDto, int userId);
    }
}