using HireMindsAPI.DTOs;
using HireMindsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HireMindsAPI.Services
{
    public class SubmissionService
    {
        private readonly AppDbContext _db;
        private readonly AIFeedbackService _aiService;

        public SubmissionService(AppDbContext db, AIFeedbackService aiService)
        {
            _db = db;
            _aiService = aiService;
        }

        // ==================== SUBMIT A TEST ====================
        public async Task<ResultResponseDto> SubmitTestAsync(int candidateId, int applicationId, SubmitTestDto dto)
        {
            // --- a. Verify Application ---
            var application = await _db.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.CandidateId == candidateId);

            if (application == null)
            {
                throw new KeyNotFoundException("Application not found.");
            }
            if (application.Status != "TestPending")
            {
                throw new InvalidOperationException("This test has already been submitted or is no longer pending.");
            }

            // --- b. Get Test Questions & Correct Answers ---
            // Fetch the test with questions and options so we can grade the submission
            var test = await _db.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == application.Job.TestId);

            if (test == null) throw new KeyNotFoundException("Test details not found.");

            int score = 0;
            int totalQuestions = test.Questions.Count;
            
            // To track topic strengths for AI feedback
            var topicScores = new Dictionary<string, TopicScoreTracker>();

            // Initialize topic trackers based on all questions in the test
            foreach (var q in test.Questions)
            {
                if (!topicScores.ContainsKey(q.Topic))
                {
                    topicScores[q.Topic] = new TopicScoreTracker { Total = 0, Correct = 0 };
                }
                topicScores[q.Topic].Total++;
            }

            // --- c & d. Loop Answers, Score, and Save ---
            var answerEntities = new List<Answer>();

            foreach (var submittedAnswer in dto.Answers)
            {
                var question = test.Questions.FirstOrDefault(q => q.Id == submittedAnswer.QuestionId);
                if (question == null) continue; // Skip invalid questions

                // Find if the selected option is correct
                var selectedOption = question.Options.FirstOrDefault(o => o.Id == submittedAnswer.SelectedOptionId);
                bool isCorrect = selectedOption?.IsCorrect ?? false;

                if (isCorrect)
                {
                    score++;
                    topicScores[question.Topic].Correct++;
                }

                // Create the Answer record to save in DB
                answerEntities.Add(new Answer
                {
                    ApplicationId = applicationId,
                    QuestionId = submittedAnswer.QuestionId,
                    SelectedOptionId = submittedAnswer.SelectedOptionId,
                    IsCorrect = isCorrect
                });
            }

            // Save all answers in one go
            _db.Answers.AddRange(answerEntities);

            // --- e. Group Topics for AI Feedback ---
            var strongTopics = new List<string>();
            var weakTopics = new List<string>();
            var topicWiseResponse = new Dictionary<string, TopicScoreDto>();

            foreach (var kvp in topicScores)
            {
                string topic = kvp.Key;
                var tracker = kvp.Value;
                
                // Avoid divide by zero
                double percentage = tracker.Total > 0 ? ((double)tracker.Correct / tracker.Total) * 100 : 0;

                topicWiseResponse[topic] = new TopicScoreDto
                {
                    Correct = tracker.Correct,
                    Total = tracker.Total,
                    Percentage = Math.Round(percentage, 2)
                };

                // Classify as strong (>= 70%) or weak (< 70%)
                if (percentage >= 70) strongTopics.Add(topic);
                else weakTopics.Add(topic);
            }

            // --- f. Call Google Gemini AI ---
            string aiFeedback = await _aiService.GetFeedbackAsync(
                application.Job.Title, 
                score, 
                totalQuestions, 
                strongTopics, 
                weakTopics
            );

            // --- g. Update Application Record ---
            application.Status = "TestTaken";
            application.Score = score;
            application.TotalQuestions = totalQuestions;
            application.Aifeedback = aiFeedback; // DB column is Aifeedback
            application.TestTakenAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // --- h. Return Result ---
            return new ResultResponseDto
            {
                Score = score,
                TotalQuestions = totalQuestions,
                Percentage = Math.Round(((double)score / totalQuestions) * 100, 2),
                AIFeedback = aiFeedback,
                Status = application.Status,
                TopicWiseScore = topicWiseResponse
            };
        }

        // ==================== GET RESULT BY APPLICATION ID ====================
        public async Task<ResultResponseDto> GetResultAsync(int applicationId)
        {
            var application = await _db.Applications
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.Question)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null) throw new KeyNotFoundException("Application not found.");
            if (application.Status == "Applied" || application.Status == "TestPending")
            {
                throw new InvalidOperationException("Test has not been taken yet.");
            }

            // Calculate Topic Wise Score from saved answers
            var topicScores = new Dictionary<string, TopicScoreTracker>();
            
            foreach (var ans in application.Answers)
            {
                if (ans.Question == null) continue;
                
                if (!topicScores.ContainsKey(ans.Question.Topic))
                {
                    topicScores[ans.Question.Topic] = new TopicScoreTracker { Total = 0, Correct = 0 };
                }
                
                topicScores[ans.Question.Topic].Total++;
                if (ans.IsCorrect == true)
                {
                    topicScores[ans.Question.Topic].Correct++;
                }
            }

            var topicWiseResponse = new Dictionary<string, TopicScoreDto>();
            foreach (var kvp in topicScores)
            {
                var tracker = kvp.Value;
                topicWiseResponse[kvp.Key] = new TopicScoreDto
                {
                    Correct = tracker.Correct,
                    Total = tracker.Total,
                    Percentage = Math.Round(tracker.Total > 0 ? ((double)tracker.Correct / tracker.Total) * 100 : 0, 2)
                };
            }

            int score = application.Score ?? 0;
            int total = application.TotalQuestions ?? 1;

            return new ResultResponseDto
            {
                Score = score,
                TotalQuestions = total,
                Percentage = Math.Round(((double)score / total) * 100, 2),
                AIFeedback = application.Aifeedback,
                Status = application.Status,
                TopicWiseScore = topicWiseResponse
            };
        }

        // Helper class to track scores before mapping to DTO
        private class TopicScoreTracker
        {
            public int Total { get; set; }
            public int Correct { get; set; }
        }
    }
}
