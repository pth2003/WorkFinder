using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class SavedJobController : Controller
    {
        private readonly ISaveJobRepository _saveJobRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IAuthService _authService;

        public SavedJobController(
            ISaveJobRepository saveJobRepository,
            IJobRepository jobRepository,
            IAuthService authService)
        {
            _saveJobRepository = saveJobRepository;
            _jobRepository = jobRepository;
            _authService = authService;
        }

        [HttpPost("save/{jobId}")]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            var job = await _jobRepository.GetJobWithDetailsAsync(jobId);
            if (job == null)
                return NotFound("Job not found");

            // Check if job is already saved
            var existingSavedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
            if (existingSavedJobs.Any(sj => sj.JobId == jobId))
                return Ok(new { success = true, message = "Job already saved" });

            var savedJob = new SavedJob
            {
                JobId = jobId,
                UserId = user.Id,
                SavedDate = DateTime.UtcNow
            };

            var result = await _saveJobRepository.CreateSavedJobAsync(savedJob);
            if (result)
                return Ok(new { success = true, message = "Job saved successfully" });

            return BadRequest(new { success = false, message = "Failed to save job" });
        }

        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveSavedJob(int id)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            // Verify the saved job belongs to the current user
            var savedJob = await _saveJobRepository.GetSavedJobByIdAsync(id);
            if (savedJob == null)
                return NotFound();

            if (savedJob.UserId != user.Id)
                return Forbid();

            var result = await _saveJobRepository.DeleteSavedJobAsync(id);
            if (result)
                return Ok(new { success = true, message = "Saved job removed successfully" });

            return BadRequest(new { success = false, message = "Failed to remove saved job" });
        }

        [HttpPost("toggle/{jobId}")]
        public async Task<IActionResult> ToggleSavedJob(int jobId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            var job = await _jobRepository.GetJobWithDetailsAsync(jobId);
            if (job == null)
                return NotFound("Job not found");

            // Check if job is already saved
            var savedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
            var existingSavedJob = savedJobs.FirstOrDefault(sj => sj.JobId == jobId);

            if (existingSavedJob != null)
            {
                // If job is already saved, remove it
                var result = await _saveJobRepository.DeleteSavedJobAsync(existingSavedJob.Id);
                if (result)
                    return Ok(new { success = true, isSaved = false, message = "Job removed from saved jobs" });

                return BadRequest(new { success = false, message = "Failed to remove saved job" });
            }
            else
            {
                // If job is not saved, save it
                var savedJob = new SavedJob
                {
                    JobId = jobId,
                    UserId = user.Id,
                    SavedDate = DateTime.UtcNow
                };

                var result = await _saveJobRepository.CreateSavedJobAsync(savedJob);
                if (result)
                    return Ok(new { success = true, isSaved = true, message = "Job saved successfully" });

                return BadRequest(new { success = false, message = "Failed to save job" });
            }
        }

        [HttpGet("check/{jobId}")]
        public async Task<IActionResult> CheckJobSaved(int jobId)
        {
            if (!User.Identity.IsAuthenticated)
                return Ok(new { isSaved = false });

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Ok(new { isSaved = false });

            var savedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
            var isSaved = savedJobs.Any(sj => sj.JobId == jobId);

            return Ok(new { isSaved });
        }
    }

    // API Controller cho SavedJob
    [Route("api/[controller]")]
    [ApiController]
    public class SavedJobApiController : ControllerBase
    {
        private readonly ISaveJobRepository _saveJobRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IAuthService _authService;

        public SavedJobApiController(
            ISaveJobRepository saveJobRepository,
            IJobRepository jobRepository,
            IAuthService authService)
        {
            _saveJobRepository = saveJobRepository;
            _jobRepository = jobRepository;
            _authService = authService;
        }

        // Lấy tất cả job đã lưu của user hiện tại
        [HttpGet("checkSaved")]
        [Authorize]
        public async Task<IActionResult> GetSavedJobIds()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            var savedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
            var jobIds = savedJobs.Select(sj => sj.JobId).ToList();

            return Ok(jobIds);
        }

        // Lưu job
        [HttpPost("save/{jobId}")]
        [Authorize]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            var job = await _jobRepository.GetJobWithDetailsAsync(jobId);
            if (job == null)
                return NotFound(new { success = false, message = "Job not found" });

            // Kiểm tra job đã được lưu chưa
            var existingSavedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
            if (existingSavedJobs.Any(sj => sj.JobId == jobId))
                return Ok(new { success = true, message = "Job already saved" });

            var savedJob = new SavedJob
            {
                JobId = jobId,
                UserId = user.Id,
                SavedDate = DateTime.UtcNow
            };

            var result = await _saveJobRepository.CreateSavedJobAsync(savedJob);
            if (result)
                return Ok(new { success = true, message = "Job saved successfully" });

            return BadRequest(new { success = false, message = "Failed to save job" });
        }

        // Xóa job khỏi danh sách đã lưu
        [HttpPost("unsave/{jobId}")]
        [Authorize]
        public async Task<IActionResult> UnsaveJob(int jobId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            // Tìm saved job record
            var savedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
            var existingSavedJob = savedJobs.FirstOrDefault(sj => sj.JobId == jobId);

            if (existingSavedJob == null)
                return Ok(new { success = true, message = "Job is not in saved list" });

            var result = await _saveJobRepository.DeleteSavedJobAsync(existingSavedJob.Id);
            if (result)
                return Ok(new { success = true, message = "Job removed from saved list" });

            return BadRequest(new { success = false, message = "Failed to remove job from saved list" });
        }

        // Kiểm tra job có được lưu hay không
        [HttpGet("check/{jobId}")]
        [Authorize]
        public async Task<IActionResult> CheckJobSaved(int jobId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized();

            var savedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
            var isSaved = savedJobs.Any(sj => sj.JobId == jobId);

            return Ok(new { isSaved });
        }
    }
}