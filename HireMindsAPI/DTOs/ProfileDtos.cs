namespace HireMindsAPI.DTOs
{
    // Used when a candidate updates their profile
    public class ProfileDto
    {
        public string Degree { get; set; } = string.Empty;     // B.Tech, M.Tech, BCA, MCA
        public string Branch { get; set; } = string.Empty;     // CS, IT, ECE, EE
        public int GraduationYear { get; set; }
        public string Skills { get; set; } = string.Empty;     // Comma-separated: "C#,Angular,SQL"
        public string CollegeName { get; set; } = string.Empty;
        public decimal? SGPA { get; set; }
        public string TenthSchoolName { get; set; } = string.Empty;
        public decimal? TenthScore { get; set; }
        public string TwelfthSchoolName { get; set; } = string.Empty;
        public decimal? TwelfthScore { get; set; }
        public string Certifications { get; set; } = string.Empty; // JSON String
        public string Projects { get; set; } = string.Empty;       // JSON String
        public string ResumeUrl { get; set; } = string.Empty;      // Passed back from upload API
    }

    // Returned when fetching a candidate's profile
    public class ProfileResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string Skills { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public decimal? SGPA { get; set; }
        public string TenthSchoolName { get; set; } = string.Empty;
        public decimal? TenthScore { get; set; }
        public string TwelfthSchoolName { get; set; } = string.Empty;
        public decimal? TwelfthScore { get; set; }
        public string ResumeUrl { get; set; } = string.Empty;
        public string Certifications { get; set; } = string.Empty;
        public string Projects { get; set; } = string.Empty;
    }
}
