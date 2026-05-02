using HireMindsAPI.DTOs;
using HireMindsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HireMindsAPI.Services
{
    public class ProfileService
    {
        private readonly AppDbContext _db;

        public ProfileService(AppDbContext db)
        {
            _db = db;
        }

        // ==================== GET PROFILE ====================
        public async Task<ProfileResponseDto> GetProfileAsync(int userId)
        {
            // Find the candidate profile and include the User data (for Name and Email)
            var profile = await _db.Candidateprofiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // If profile doesn't exist, throw a 404 (handled by our middleware)
            if (profile == null)
            {
                throw new KeyNotFoundException("Profile not found.");
            }

            // Map database entity to DTO
            return new ProfileResponseDto
            {
                Id = profile.Id,
                FullName = profile.User.FullName,
                Email = profile.User.Email,
                Degree = profile.Degree ?? "",
                Branch = profile.Branch ?? "",
                GraduationYear = profile.GraduationYear ?? 0,
                Skills = profile.Skills ?? "",
                CollegeName = profile.CollegeName ?? "",
                SGPA = profile.SGPA,
                TenthSchoolName = profile.TenthSchoolName ?? "",
                TenthScore = profile.TenthScore,
                TwelfthSchoolName = profile.TwelfthSchoolName ?? "",
                TwelfthScore = profile.TwelfthScore,
                ResumeUrl = profile.ResumeUrl ?? "",
                Certifications = profile.Certifications ?? "",
                Projects = profile.Projects ?? ""
            };
        }

        // ==================== UPDATE PROFILE ====================
        public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, ProfileDto dto)
        {
            // Find the candidate profile
            var profile = await _db.Candidateprofiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                throw new KeyNotFoundException("Profile not found.");
            }

            // Update the fields
            profile.Degree = dto.Degree;
            profile.Branch = dto.Branch;
            profile.GraduationYear = dto.GraduationYear;
            profile.Skills = dto.Skills;
            profile.CollegeName = dto.CollegeName;
            profile.SGPA = dto.SGPA;
            profile.TenthSchoolName = dto.TenthSchoolName;
            profile.TenthScore = dto.TenthScore;
            profile.TwelfthSchoolName = dto.TwelfthSchoolName;
            profile.TwelfthScore = dto.TwelfthScore;
            profile.ResumeUrl = dto.ResumeUrl;
            profile.Certifications = dto.Certifications;
            profile.Projects = dto.Projects;

            // Save changes to database
            await _db.SaveChangesAsync();

            // Return the updated profile
            return new ProfileResponseDto
            {
                Id = profile.Id,
                FullName = profile.User.FullName,
                Email = profile.User.Email,
                Degree = profile.Degree ?? "",
                Branch = profile.Branch ?? "",
                GraduationYear = profile.GraduationYear ?? 0,
                Skills = profile.Skills ?? "",
                CollegeName = profile.CollegeName ?? "",
                SGPA = profile.SGPA,
                TenthSchoolName = profile.TenthSchoolName ?? "",
                TenthScore = profile.TenthScore,
                TwelfthSchoolName = profile.TwelfthSchoolName ?? "",
                TwelfthScore = profile.TwelfthScore,
                ResumeUrl = profile.ResumeUrl ?? "",
                Certifications = profile.Certifications ?? "",
                Projects = profile.Projects ?? ""
            };
        }
    }
}
