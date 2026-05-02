using System;
using System.Collections.Generic;

namespace HireMindsAPI.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public bool? IsCorrect { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual Option? SelectedOption { get; set; }
}
