using System.Security.Claims;
using HireMindsAPI.DTOs;
using HireMindsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMindsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Only Candidates can access these endpoints
    [Authorize(Roles = "Candidate")]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;

        public ProfileController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET: api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            // Extract the UserId from the JWT token claims
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user token." });
            }

            var result = await _profileService.GetProfileAsync(userId);
            return Ok(result);
        }

        // PUT: api/profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto dto)
        {
            // Extract the UserId from the JWT token claims
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user token." });
            }

            var result = await _profileService.UpdateProfileAsync(userId, dto);
            return Ok(result);
        }
        // POST: api/profile/resume
        [HttpPost("resume")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file uploaded." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { Message = "File size exceeds 5MB limit." });

            if (file.ContentType != "application/pdf")
                return BadRequest(new { Message = "Only PDF files are allowed." });

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Create unique filename
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/resumes/{uniqueFileName}";
            return Ok(new { url = fileUrl });
        }
    }
}
