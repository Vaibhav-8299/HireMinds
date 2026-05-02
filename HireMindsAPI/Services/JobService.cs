using HireMindsAPI.DTOs;
using HireMindsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HireMindsAPI.Services
{
    public class JobService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public JobService(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // ==================== CREATE A NEW JOB ====================
        public async Task<int> CreateJobAsync(int recruiterId, CreateJobDto dto)
        {
            var job = new Job
            {
                RecruiterId = recruiterId,
                Title = dto.Title,
                Description = dto.Description,
                CompanyName = dto.CompanyName,
                Location = dto.Location,
                RequiredDegree = dto.RequiredDegree,
                RequiredBranch = dto.RequiredBranch,
                TestId = dto.TestId,
                MinBTechScore = dto.MinBTechScore,
                Min10thScore = dto.Min10thScore,
                Min12thScore = dto.Min12thScore,
                IsActive = true,
                PostedAt = DateTime.UtcNow
            };

            _db.Jobs.Add(job);
            await _db.SaveChangesAsync();

            // Clear cache related to jobs so fresh data shows up
            // Note: Since we have dynamic cache keys based on filters, it's easier in a simple setup 
            // to just let it expire in 5 minutes, but for a smaller app, we could clear a specific key.
            // A common pattern is using a "cache cancellation token" to clear all job-related caches.
            // For simplicity here, we rely on the 5 min expiration, but we'll remove the blank search key just in case.
            _cache.Remove("jobs_all___");

            return job.Id;
        }

        // ==================== GET ALL JOBS (WITH CACHING) ====================
        public async Task<List<JobResponseDto>> GetJobsAsync(string? search, string? location, string? company)
        {
            // Build a unique cache key based on the search filters
            string cacheKey = $"jobs_all_{search}_{location}_{company}";

            // Check if results are already in memory cache
            if (_cache.TryGetValue(cacheKey, out List<JobResponseDto>? cachedJobs))
            {
                return cachedJobs!;
            }

            // If not in cache, query the database
            var query = _db.Jobs
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .Where(j => j.IsActive == true)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(j => j.Title.Contains(search) || j.Description!.Contains(search));
            }
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(j => j.Location.Contains(location));
            }
            if (!string.IsNullOrWhiteSpace(company))
            {
                query = query.Where(j => j.CompanyName.Contains(company));
            }

            // Execute query and map to DTO
            var jobs = await query
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new JobResponseDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description ?? "",
                    CompanyName = j.CompanyName,
                    Location = j.Location,
                    RequiredDegree = j.RequiredDegree,
                    RequiredBranch = j.RequiredBranch,
                    TestId = j.TestId,
                    MinBTechScore = j.MinBTechScore,
                    Min10thScore = j.Min10thScore,
                    Min12thScore = j.Min12thScore,
                    IsActive = j.IsActive ?? false,
                    PostedAt = j.PostedAt ?? DateTime.UtcNow,
                    RecruiterName = j.Recruiter.FullName,
                    ApplicationCount = j.Applications.Count
                })
                .ToListAsync();

            // Save results to cache for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, jobs, cacheOptions);

            return jobs;
        }

        // ==================== GET RECRUITER'S OWN JOBS ====================
        public async Task<List<JobResponseDto>> GetMyJobsAsync(int recruiterId)
        {
            return await _db.Jobs
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .Where(j => j.RecruiterId == recruiterId)
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new JobResponseDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description ?? "",
                    CompanyName = j.CompanyName,
                    Location = j.Location,
                    RequiredDegree = j.RequiredDegree,
                    RequiredBranch = j.RequiredBranch,
                    TestId = j.TestId,
                    MinBTechScore = j.MinBTechScore,
                    Min10thScore = j.Min10thScore,
                    Min12thScore = j.Min12thScore,
                    IsActive = j.IsActive ?? false,
                    PostedAt = j.PostedAt ?? DateTime.UtcNow,
                    RecruiterName = j.Recruiter.FullName,
                    ApplicationCount = j.Applications.Count
                })
                .ToListAsync();
        }

        // ==================== GET JOB BY ID ====================
        public async Task<JobResponseDto> GetJobByIdAsync(int jobId)
        {
            var job = await _db.Jobs
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
            {
                throw new KeyNotFoundException("Job not found.");
            }

            return new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description ?? "",
                CompanyName = job.CompanyName,
                Location = job.Location,
                RequiredDegree = job.RequiredDegree,
                RequiredBranch = job.RequiredBranch,
                TestId = job.TestId,
                MinBTechScore = job.MinBTechScore,
                Min10thScore = job.Min10thScore,
                Min12thScore = job.Min12thScore,
                IsActive = job.IsActive ?? false,
                PostedAt = job.PostedAt ?? DateTime.UtcNow,
                RecruiterName = job.Recruiter.FullName,
                ApplicationCount = job.Applications.Count
            };
        }
    }
}
