namespace HireMindsAPI.DTOs
{
    // One answer submitted by the candidate
    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }

    // The full test submission (list of all answers)
    public class SubmitTestDto
    {
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }

    // Returned after test is submitted and scored
    public class ResultResponseDto
    {
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public string? AIFeedback { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, TopicScoreDto> TopicWiseScore { get; set; } = new();
    }

    // Score breakdown per topic
    public class TopicScoreDto
    {
        public int Correct { get; set; }
        public int Total { get; set; }
        public double Percentage { get; set; }
    }
}
