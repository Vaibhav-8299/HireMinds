using System;
using System.Collections.Generic;

namespace HireMindsAPI.Models;

public partial class Candidateprofile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Degree { get; set; }

    public string? Branch { get; set; }

    public int? GraduationYear { get; set; }

    public string? Skills { get; set; }

    public string? CollegeName { get; set; }
    public decimal? SGPA { get; set; }
    public string? TenthSchoolName { get; set; }
    public decimal? TenthScore { get; set; }
    public string? TwelfthSchoolName { get; set; }
    public decimal? TwelfthScore { get; set; }
    public string? ResumeUrl { get; set; }
    public string? Certifications { get; set; }
    public string? Projects { get; set; }

    public virtual User User { get; set; } = null!;
}
