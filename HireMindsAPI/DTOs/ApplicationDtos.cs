namespace HireMindsAPI.DTOs
{
    // Returned when fetching application details
    public class ApplicationResponseDto
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? Score { get; set; }
        public int? TotalQuestions { get; set; }
        public string? AIFeedback { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? TestTakenAt { get; set; }
        public int? TestId { get; set; }

        // Candidate profile info (for recruiter to view)
        public string? Degree { get; set; }
        public string? Branch { get; set; }
        public string? Skills { get; set; }
        public string? SelectionMessage { get; set; }
        public string? CollegeName { get; set; }
        public decimal? SGPA { get; set; }
        public string? TenthSchoolName { get; set; }
        public decimal? TenthScore { get; set; }
        public string? TwelfthSchoolName { get; set; }
        public decimal? TwelfthScore { get; set; }
        public string? ResumeUrl { get; set; }
        public string? Certifications { get; set; }
        public string? Projects { get; set; }
    }

    // Used when recruiter changes application status
    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;  // "Selected" or "Rejected"
        public string? Message { get; set; }
    }
}
