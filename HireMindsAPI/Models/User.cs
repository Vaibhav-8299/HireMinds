using System;
using System.Collections.Generic;

namespace HireMindsAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsEmailVerified { get; set; }
    public string? EmailOtp { get; set; }
    public DateTime? OtpExpiry { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Candidateprofile? Candidateprofile { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
