using System;
using System.Collections.Generic;

namespace HireMindsAPI.Models;

public partial class Test
{
    public int Id { get; set; }

    public int RecruiterId { get; set; }

    public string Title { get; set; } = null!;

    public int TimeLimitMinutes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual User Recruiter { get; set; } = null!;
}
