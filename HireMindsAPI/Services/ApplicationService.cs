using HireMindsAPI.DTOs;
using HireMindsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HireMindsAPI.Services
{
    public class ApplicationService
    {
        private readonly AppDbContext _db;

        public ApplicationService(AppDbContext db)
        {
            _db = db;
        }

        // ==================== APPLY FOR A JOB ====================
        public async Task<int> ApplyAsync(int candidateId, int jobId)
        {
            // 1. Fetch Job and Candidate Profile
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            var profile = await _db.Candidateprofiles.FirstOrDefaultAsync(p => p.UserId == candidateId);

            if (job == null) throw new KeyNotFoundException("Job not found.");
            if (profile == null) throw new KeyNotFoundException("Candidate profile not found.");
            if (job.IsActive != true) throw new InvalidOperationException("This job is no longer active.");

            // 2. ELIGIBILITY VALIDATION - MOST IMPORTANT PART
            // If the job requires specific Degrees, check if candidate's degree is in the list
            if (!string.IsNullOrWhiteSpace(job.RequiredDegree) && !job.RequiredDegree.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
                var allowedDegrees = job.RequiredDegree.Split(',').Select(d => d.Trim()).ToList();
                if (string.IsNullOrWhiteSpace(profile.Degree) || 
                    !allowedDegrees.Contains(profile.Degree, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Eligibility failed: This job requires one of ({job.RequiredDegree}) degrees, but you have a {profile.Degree ?? "None"}.");
                }
            }

            // If the job requires specific Branches, check if candidate's branch is in the list
            if (!string.IsNullOrWhiteSpace(job.RequiredBranch) && !job.RequiredBranch.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
                var allowedBranches = job.RequiredBranch.Split(',').Select(b => b.Trim()).ToList();
                if (string.IsNullOrWhiteSpace(profile.Branch) || 
                    !allowedBranches.Contains(profile.Branch, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Eligibility failed: This job requires one of ({job.RequiredBranch}) branches, but you have a {profile.Branch ?? "None"}.");
                }
            }

            // --- New Score Eligibility Checks ---
            if (job.MinBTechScore.HasValue && (profile.SGPA ?? 0) < job.MinBTechScore.Value)
            {
                throw new ArgumentException($"Eligibility failed: Minimum {job.MinBTechScore}% in B.Tech is required. Your profile shows {profile.SGPA ?? 0}%.");
            }

            if (job.Min10thScore.HasValue && (profile.TenthScore ?? 0) < job.Min10thScore.Value)
            {
                throw new ArgumentException($"Eligibility failed: Minimum {job.Min10thScore}% in 10th is required. Your profile shows {profile.TenthScore ?? 0}%.");
            }

            if (job.Min12thScore.HasValue && (profile.TwelfthScore ?? 0) < job.Min12thScore.Value)
            {
                throw new ArgumentException($"Eligibility failed: Minimum {job.Min12thScore}% in 12th is required. Your profile shows {profile.TwelfthScore ?? 0}%.");
            }

            // 3. Cooldown / Duplicate check
            var existingApplication = await _db.Applications
                .FirstOrDefaultAsync(a => a.CandidateId == candidateId && a.JobId == jobId);

            if (existingApplication != null)
            {
                // Check if applied within the last 3 months
                if (existingApplication.AppliedAt > DateTime.UtcNow.AddMonths(-3))
                {
                    throw new InvalidOperationException("You have recently appeared for this job. You will be eligible again after 3 months.");
                }

                // More than 3 months ago: Reset the application to allow re-taking the test
                // Delete old answers
                var oldAnswers = await _db.Answers.Where(a => a.ApplicationId == existingApplication.Id).ToListAsync();
                _db.Answers.RemoveRange(oldAnswers);

                existingApplication.Status = "TestPending";
                existingApplication.Score = null;
                existingApplication.TotalQuestions = null;
                existingApplication.Aifeedback = null;
                existingApplication.SelectionMessage = null;
                existingApplication.TestTakenAt = null;
                existingApplication.AppliedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return existingApplication.Id;
            }

            // 4. Create Application
            var application = new Application
            {
                CandidateId = candidateId,
                JobId = jobId,
                Status = "TestPending", // Standard initial status
                AppliedAt = DateTime.UtcNow
            };

            _db.Applications.Add(application);
            await _db.SaveChangesAsync();

            return application.Id;
        }

        // ==================== GET MY APPLICATIONS (Candidate View) ====================
        public async Task<List<ApplicationResponseDto>> GetMyApplicationsAsync(int candidateId)
        {
            return await _db.Applications
                .Include(a => a.Job)
                .Where(a => a.CandidateId == candidateId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new ApplicationResponseDto
                {
                    Id = a.Id,
                    CandidateId = a.CandidateId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    CompanyName = a.Job.CompanyName,
                    Location = a.Job.Location,
                    Status = a.Status ?? "Applied",
                    Score = a.Score,
                    TotalQuestions = a.TotalQuestions,
                    AIFeedback = a.Aifeedback,
                    AppliedAt = a.AppliedAt ?? DateTime.UtcNow,
                    TestTakenAt = a.TestTakenAt,
                    TestId = a.Job.TestId,
                    SelectionMessage = a.SelectionMessage
                })
                .ToListAsync();
        }

        // ==================== GET APPLICATIONS BY JOB (Recruiter View) ====================
        public async Task<List<ApplicationResponseDto>> GetApplicationsByJobAsync(int jobId, int recruiterId)
        {
            // Verify job belongs to this recruiter
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiterId);
            if (job == null)
            {
                throw new UnauthorizedAccessException("Job not found or you don't have permission to view its applications.");
            }

            return await _db.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(u => u.Candidateprofile)
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.Score) // Order by best score first!
                .Select(a => new ApplicationResponseDto
                {
                    Id = a.Id,
                    CandidateId = a.CandidateId,
                    CandidateName = a.Candidate.FullName,
                    JobId = a.JobId,
                    Status = a.Status ?? "Applied",
                    Score = a.Score,
                    TotalQuestions = a.TotalQuestions,
                    AIFeedback = a.Aifeedback,
                    AppliedAt = a.AppliedAt ?? DateTime.UtcNow,
                    TestTakenAt = a.TestTakenAt,
                    
                    // Profile details for recruiter to review
                    Degree = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.Degree : null,
                    Branch = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.Branch : null,
                    Skills = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.Skills : null,
                    SelectionMessage = a.SelectionMessage,
                    CollegeName = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.CollegeName : null,
                    SGPA = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.SGPA : null,
                    TenthSchoolName = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.TenthSchoolName : null,
                    TenthScore = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.TenthScore : null,
                    TwelfthSchoolName = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.TwelfthSchoolName : null,
                    TwelfthScore = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.TwelfthScore : null,
                    ResumeUrl = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.ResumeUrl : null,
                    Certifications = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.Certifications : null,
                    Projects = a.Candidate.Candidateprofile != null ? a.Candidate.Candidateprofile.Projects : null
                })
                .ToListAsync();
        }

        // ==================== GET SINGLE APPLICATION ====================
        public async Task<ApplicationResponseDto> GetApplicationByIdAsync(int applicationId)
        {
            var app = await _db.Applications
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                    .ThenInclude(u => u.Candidateprofile)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null) throw new KeyNotFoundException("Application not found.");

            return new ApplicationResponseDto
            {
                Id = app.Id,
                CandidateId = app.CandidateId,
                CandidateName = app.Candidate.FullName,
                JobId = app.JobId,
                JobTitle = app.Job.Title,
                CompanyName = app.Job.CompanyName,
                Location = app.Job.Location,
                Status = app.Status ?? "Applied",
                Score = app.Score,
                TotalQuestions = app.TotalQuestions,
                AIFeedback = app.Aifeedback,
                AppliedAt = app.AppliedAt ?? DateTime.UtcNow,
                TestTakenAt = app.TestTakenAt,
                TestId = app.Job.TestId,
                
                Degree = app.Candidate.Candidateprofile?.Degree,
                Branch = app.Candidate.Candidateprofile?.Branch,
                Skills = app.Candidate.Candidateprofile?.Skills,
                SelectionMessage = app.SelectionMessage
            };
        }

        // ==================== UPDATE STATUS (Recruiter Action) ====================
        public async Task<string> UpdateStatusAsync(int applicationId, int recruiterId, string status, string? message)
        {
            if (status != "Selected" && status != "Rejected")
            {
                throw new ArgumentException("Status must be 'Selected' or 'Rejected'.");
            }

            var app = await _db.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null) throw new KeyNotFoundException("Application not found.");

            if (app.Job.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this application.");
            }

            app.Status = status;
            app.SelectionMessage = message;
            await _db.SaveChangesAsync();

            return $"Application status updated to {status}.";
        }
    }
}
