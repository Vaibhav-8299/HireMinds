using System.Security.Claims;
using HireMindsAPI.DTOs;
using HireMindsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMindsAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class SubmissionController : ControllerBase
    {
        private readonly SubmissionService _submissionService;

        public SubmissionController(SubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        // POST: api/submit/{applicationId}
        [HttpPost("submit/{applicationId}")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> SubmitTest(int applicationId, [FromBody] SubmitTestDto dto)
        {
            var candidateId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _submissionService.SubmitTestAsync(candidateId, applicationId, dto);
            return Ok(result);
        }

        // GET: api/result/{applicationId}
        // Both Recruiters and Candidates can view the result
        [HttpGet("result/{applicationId}")]
        [Authorize]
        public async Task<IActionResult> GetResult(int applicationId)
        {
            var result = await _submissionService.GetResultAsync(applicationId);
            return Ok(result);
        }
    }
}
