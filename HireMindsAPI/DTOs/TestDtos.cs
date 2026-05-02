namespace HireMindsAPI.DTOs
{
    // Used when a recruiter creates a new test
    public class CreateTestDto
    {
        public string Title { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; }
    }

    // Used when adding a question to a test
    public class AddQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";  // Easy, Medium, Hard
        public List<OptionDto> Options { get; set; } = new();
    }

    // Used when updating test metadata
    public class UpdateTestDto
    {
        public string Title { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; }
    }

    // Used when updating an existing question
    public class UpdateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public List<OptionDto> Options { get; set; } = new();
    }

    // Each option for a question
    public class OptionDto
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    // Returned when fetching a test with all questions
    public class TestResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RecruiterName { get; set; } = string.Empty;
        public List<QuestionResponseDto> Questions { get; set; } = new();
    }

    // Each question inside a test response
    public class QuestionResponseDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public List<OptionResponseDto> Options { get; set; } = new();
    }

    // Each option inside a question response
    public class OptionResponseDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        // Note: IsCorrect is NOT included here — candidates should not see correct answers!
    }
}
