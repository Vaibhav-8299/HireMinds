namespace HireMindsAPI.DTOs
{
    // Used when a recruiter posts a new job
    public class CreateJobDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? RequiredDegree { get; set; }   // B.Tech, M.Tech, BCA, MCA, or null = Any
        public string? RequiredBranch { get; set; }   // CS, IT, ECE, EE, or null = Any
        public int? TestId { get; set; }              // Link to an assessment test
        public decimal? MinBTechScore { get; set; }
        public decimal? Min10thScore { get; set; }
        public decimal? Min12thScore { get; set; }
    }

    // Returned when fetching job details
    public class JobResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? RequiredDegree { get; set; }
        public string? RequiredBranch { get; set; }
        public int? TestId { get; set; }
        public decimal? MinBTechScore { get; set; }
        public decimal? Min10thScore { get; set; }
        public decimal? Min12thScore { get; set; }
        public bool IsActive { get; set; }
        public DateTime PostedAt { get; set; }
        public string RecruiterName { get; set; } = string.Empty;
        public int ApplicationCount { get; set; }    // How many candidates applied
    }
}
