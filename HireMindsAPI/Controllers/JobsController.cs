using System.Security.Claims;
using HireMindsAPI.DTOs;
using HireMindsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMindsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly JobService _jobService;

        public JobsController(JobService jobService)
        {
            _jobService = jobService;
        }

        // POST: api/jobs
        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var jobId = await _jobService.CreateJobAsync(recruiterId, dto);
            return Ok(new { Message = "Job created successfully.", JobId = jobId });
        }

        // GET: api/jobs?search=&location=&company=
        // Any logged-in user can search jobs
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetJobs([FromQuery] string? search, [FromQuery] string? location, [FromQuery] string? company)
        {
            var result = await _jobService.GetJobsAsync(search, location, company);
            return Ok(result);
        }

        // GET: api/jobs/my
        // Recruiter sees jobs they posted
        [HttpGet("my")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetMyJobs()
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _jobService.GetMyJobsAsync(recruiterId);
            return Ok(result);
        }

        // GET: api/jobs/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetJobById(int id)
        {
            var result = await _jobService.GetJobByIdAsync(id);
            return Ok(result);
        }
    }
}
