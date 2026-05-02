using System.Security.Claims;
using HireMindsAPI.DTOs;
using HireMindsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMindsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly TestService _testService;

        public TestsController(TestService testService)
        {
            _testService = testService;
        }

        // POST: api/tests
        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> CreateTest([FromBody] CreateTestDto dto)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var testId = await _testService.CreateTestAsync(recruiterId, dto);
            return Ok(new { Message = "Test created successfully.", TestId = testId });
        }

        // POST: api/tests/{testId}/questions
        [HttpPost("{testId}/questions")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> AddQuestion(int testId, [FromBody] AddQuestionDto dto)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _testService.AddQuestionAsync(testId, recruiterId, dto);
            return Ok(new { Message = result });
        }

        // PUT: api/tests/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateTest(int id, [FromBody] UpdateTestDto dto)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _testService.UpdateTestAsync(id, recruiterId, dto);
            return Ok(new { Message = result });
        }

        // PUT: api/tests/{testId}/questions/{questionId}
        [HttpPut("{testId}/questions/{questionId}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateQuestion(int testId, int questionId, [FromBody] UpdateQuestionDto dto)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _testService.UpdateQuestionAsync(testId, questionId, recruiterId, dto);
            return Ok(new { Message = result });
        }

        // DELETE: api/tests/{testId}/questions/{questionId}
        [HttpDelete("{testId}/questions/{questionId}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> DeleteQuestion(int testId, int questionId)
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _testService.DeleteQuestionAsync(testId, questionId, recruiterId);
            return Ok(new { Message = result });
        }

        // GET: api/tests/my
        [HttpGet("my")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetMyTests()
        {
            var recruiterId = int.Parse(User.FindFirstValue("UserId")!);
            var result = await _testService.GetMyTestsAsync(recruiterId);
            return Ok(result);
        }

        // GET: api/tests/{id}
        // Both Candidates and Recruiters can view tests
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTestById(int id)
        {
            var result = await _testService.GetTestByIdAsync(id);
            return Ok(result);
        }
    }
}
