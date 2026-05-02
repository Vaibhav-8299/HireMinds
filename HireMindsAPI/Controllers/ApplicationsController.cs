using System.Security.Claims;
using HireMindsAPI.DTOs;
using HireMindsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMindsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationService _applicationService;

        public ApplicationsController(ApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // POST: api/applications/{jobId}/apply
        [HttpPost("{jobId}/apply")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> Apply(int jobId)
        {
            var candidateId = int.Parse(User.FindFirstValue("UserId")!);
            var appId = await _applicationService.ApplyAsync(candidateId, jobId);
            return Ok(new { Message = "Successfully applied for the job.", ApplicationId = appId });
        }

        // GET: api/applications/my
        [HttpGet("my")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetMyApplications()
        {
            var candidateId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _applicationService.GetMyApplicationsAsync(candidateId);
            return Ok(result);
        }

        // GET: api/applications/job/{jobId}
        [HttpGet("job/{jobId}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetApplicationsByJob(int jobId)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _applicationService.GetApplicationsByJobAsync(jobId, recruiterId);
            return Ok(result);
        }

        // GET: api/applications/{id}
        // Both Candidates and Recruiters can view single application details
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetApplicationById(int id)
        {
            var result = await _applicationService.GetApplicationByIdAsync(id);
            return Ok(result);
        }

        // PUT: api/applications/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _applicationService.UpdateStatusAsync(id, recruiterId, dto.Status, dto.Message);
            return Ok(new { Message = result });
        }

        // GET: api/applications/notifications
        [HttpGet("notifications")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetNotifications()
        {
            var candidateId = int.Parse(User.FindFirstValue("UserId")!);
            // Notifications are just applications that have been Selected or Rejected
            var applications = await _applicationService.GetMyApplicationsAsync(candidateId);
            var notifications = applications
                .Where(a => a.Status == "Selected" || a.Status == "Rejected")
                .OrderByDescending(a => a.AppliedAt) // Or TestTakenAt if available
                .ToList();
            
            return Ok(notifications);
        }
    }
}
