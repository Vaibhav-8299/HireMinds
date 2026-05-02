using System;
using System.Collections.Generic;

namespace HireMindsAPI.Models;

public partial class Job
{
    public int Id { get; set; }

    public int RecruiterId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string? RequiredDegree { get; set; }

    public string? RequiredBranch { get; set; }

    public int? TestId { get; set; }
    public decimal? MinBTechScore { get; set; }
    public decimal? Min10thScore { get; set; }
    public decimal? Min12thScore { get; set; }

    public bool? IsActive { get; set; }
    public DateTime? PostedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual User Recruiter { get; set; } = null!;

    public virtual Test? Test { get; set; }
}
