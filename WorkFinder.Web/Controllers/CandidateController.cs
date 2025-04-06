using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using WorkFinder.Web.Data;
using WorkFinder.Web.DTOs.Candidate;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Route("/Candidate")]
    public class CandidateController : Controller
    {
        private readonly ILogger<CandidateController> _logger;
        private readonly ICandidateRepository _candidateRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CandidateController(ILogger<CandidateController> logger, ICandidateRepository candidateRepository, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _logger = logger;
            _candidateRepository = candidateRepository;
        }

        [HttpGet]
        [Route("Dashboard")]
        public async Task<IActionResult> DashboardAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Email = user.Email;
            ViewBag.NumberAppliedJob = this._candidateRepository.CountAppliedJob(user.Id);
            ViewBag.NumberSavedJob = this._candidateRepository.CountSavedJob(user.Id);
            return View();
        }

        [HttpGet]
        [Route("AppliedJobs")]
        public async Task<IActionResult> AppliedJobsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.NumberAppliedJob = this._candidateRepository.CountAppliedJob(user.Id);
            List<JobApplication> jobApplicationList = this._candidateRepository.GetAppliedJobList(user.Id);
            return View("AppliedJobs", jobApplicationList);
        }

        [HttpGet]
        [Route("FavoriteJobs")]
        public async Task<IActionResult> FavoriteJobsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.NumberSavedJob = this._candidateRepository.CountSavedJob(user.Id);
            List<Job> jobList = this._candidateRepository.GetSavedJobList(user.Id);
            return View("FavoriteJobs", jobList);
        }

        [HttpGet]
        [Route("Settings")]
        public async Task<IActionResult> SettingsAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ApplicationUser applicationUser = this._candidateRepository.GetUserByEmail(currentUser.Email);
            UserSettingDto user = new UserSettingDto()
            {
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                ProfilePicture = applicationUser.ProfilePicture,
                Email = applicationUser.Email,
                PhoneNumber = applicationUser.PhoneNumber
            };

            Resume resume = this._candidateRepository.GetResumeByUserId(currentUser.Id);
            ResumeSettingDto resumeSettingDto = new ResumeSettingDto();
            if (resume != null)
            {
                // resumeSettingDto = new ResumeSettingDto()
                // {
                resumeSettingDto.Summary = resume.Summary;
                resumeSettingDto.Skills = resume.Skills;
                resumeSettingDto.Education = resume.Education;
                resumeSettingDto.Experience = resume.Experience;
                resumeSettingDto.Certifications = resume.Certifications;
                resumeSettingDto.Languages = resume.Languages;
                resumeSettingDto.FileUrl = resume.FileUrl;

                // };
            }
            SettingViewModel settingViewModel = new SettingViewModel()
            {
                userSettingDto = user,
                resumeSettingDto = resumeSettingDto
            };
            return View("Settings", settingViewModel);
        }

        [HttpPost]
        [Route("SaveUserUpdated")]
        public IActionResult SaveUserUpdated(SettingViewModel settingViewModel)
        {
            if (settingViewModel.userSettingDto.ImageFile != null && settingViewModel.userSettingDto.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profile-picture");
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(settingViewModel.userSettingDto.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    settingViewModel.userSettingDto.ImageFile.CopyTo(stream);
                }

                settingViewModel.userSettingDto.ProfilePicture = "/img/profile-picture/" + uniqueFileName; // Lưu đường dẫn vào database
            }
            this._candidateRepository.UpdateUser(settingViewModel.userSettingDto);
            return Redirect("/Candidate/Settings");
        }

        [HttpPost]
        [Route("SaveResumeUpdated")]
        public async Task<IActionResult> SaveResumeUpdatedAsync(SettingViewModel settingViewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (settingViewModel.resumeSettingDto.ImageFile != null && settingViewModel.resumeSettingDto.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/cv-picture");
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(settingViewModel.resumeSettingDto.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    settingViewModel.resumeSettingDto.ImageFile.CopyTo(stream);
                }

                settingViewModel.resumeSettingDto.FileUrl = "/img/cv-picture/" + uniqueFileName; // Lưu đường dẫn vào database
            }
            this._candidateRepository.UpdateResume(settingViewModel.resumeSettingDto, currentUser.Id);
            return Redirect("/Candidate/Settings");
        }

        [HttpPost]
        [Route("SaveJob")]
        public async Task<IActionResult> SaveJob([FromBody] string id)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Vui long dang nhap" });
            }

            if (_candidateRepository.SaveJob(user.Id, int.Parse(id)))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("DeleteSavedJob")]
        public async Task<IActionResult> DeleteSavedJob([FromBody] string id)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Vui long dang nhap" });
            }

            if (_candidateRepository.DeleteSavedJob(user.Id, int.Parse(id)))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("CheckSavedJob")]
        public async Task<IActionResult> CheckSavedJob([FromBody] string id)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Vui long dang nhap" });
            }

            if (_candidateRepository.CheckSavedJob(user.Id, int.Parse(id)))
            {
                return Json(new { existsSavedJob = true });
            }
            else
            {
                return Json(new { existsSavedJob = false });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}