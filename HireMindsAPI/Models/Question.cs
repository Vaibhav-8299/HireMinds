using System;
using System.Collections.Generic;

namespace HireMindsAPI.Models;

public partial class Question
{
    public int Id { get; set; }

    public int TestId { get; set; }

    public string Text { get; set; } = null!;

    public string Topic { get; set; } = null!;

    public string? Difficulty { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual Test Test { get; set; } = null!;
}
