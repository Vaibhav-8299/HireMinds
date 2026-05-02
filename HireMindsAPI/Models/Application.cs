using System;
using System.Collections.Generic;

namespace HireMindsAPI.Models;

public partial class Application
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public int JobId { get; set; }

    public string? Status { get; set; }

    public int? Score { get; set; }

    public int? TotalQuestions { get; set; }

    public string? Aifeedback { get; set; }

    public string? SelectionMessage { get; set; }

    public DateTime? AppliedAt { get; set; }

    public DateTime? TestTakenAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual User Candidate { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;
}
