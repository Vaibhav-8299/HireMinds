using HireMindsAPI.DTOs;
using HireMindsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HireMindsAPI.Services
{
    public class TestService
    {
        private readonly AppDbContext _db;

        public TestService(AppDbContext db)
        {
            _db = db;
        }

        // ==================== CREATE A NEW TEST ====================
        public async Task<int> CreateTestAsync(int recruiterId, CreateTestDto dto)
        {
            var test = new Test
            {
                RecruiterId = recruiterId,
                Title = dto.Title,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            return test.Id;
        }

        // ==================== ADD A QUESTION TO A TEST ====================
        public async Task<string> AddQuestionAsync(int testId, int recruiterId, AddQuestionDto dto)
        {
            // Verify test exists and belongs to this recruiter
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testId && t.RecruiterId == recruiterId);
            if (test == null)
            {
                throw new UnauthorizedAccessException("Test not found or you don't have permission to edit it.");
            }

            // Must have options
            if (dto.Options == null || !dto.Options.Any())
            {
                throw new ArgumentException("A question must have options.");
            }

            var question = new Question
            {
                TestId = testId,
                Text = dto.Text,
                Topic = dto.Topic,
                Difficulty = dto.Difficulty,
                Options = dto.Options.Select(o => new Option
                {
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            _db.Questions.Add(question);
            await _db.SaveChangesAsync();

            return "Question added successfully.";
        }

        // ==================== UPDATE TEST METADATA ====================
        public async Task<string> UpdateTestAsync(int testId, int recruiterId, UpdateTestDto dto)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testId && t.RecruiterId == recruiterId);
            if (test == null)
            {
                throw new UnauthorizedAccessException("Test not found or you don't have permission to edit it.");
            }

            test.Title = dto.Title;
            test.TimeLimitMinutes = dto.TimeLimitMinutes;

            await _db.SaveChangesAsync();
            return "Test updated successfully.";
        }

        // ==================== UPDATE A QUESTION ====================
        public async Task<string> UpdateQuestionAsync(int testId, int questionId, int recruiterId, UpdateQuestionDto dto)
        {
            // First verify the test belongs to the recruiter
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testId && t.RecruiterId == recruiterId);
            if (test == null)
            {
                throw new UnauthorizedAccessException("Test not found or you don't have permission to edit it.");
            }

            var question = await _db.Questions.Include(q => q.Options)
                                              .FirstOrDefaultAsync(q => q.Id == questionId && q.TestId == testId);
            if (question == null)
            {
                throw new KeyNotFoundException("Question not found.");
            }

            if (dto.Options == null || !dto.Options.Any())
            {
                throw new ArgumentException("A question must have options.");
            }

            question.Text = dto.Text;
            question.Topic = dto.Topic;
            question.Difficulty = dto.Difficulty;

            // Drop existing options and recreate
            _db.Options.RemoveRange(question.Options);

            question.Options = dto.Options.Select(o => new Option
            {
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList();

            await _db.SaveChangesAsync();
            return "Question updated successfully.";
        }

        // ==================== DELETE A QUESTION ====================
        public async Task<string> DeleteQuestionAsync(int testId, int questionId, int recruiterId)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testId && t.RecruiterId == recruiterId);
            if (test == null)
            {
                throw new UnauthorizedAccessException("Test not found or you don't have permission to edit it.");
            }

            var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == questionId && q.TestId == testId);
            if (question == null)
            {
                throw new KeyNotFoundException("Question not found.");
            }

            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();

            return "Question deleted successfully.";
        }

        // ==================== GET ALL TESTS FOR A RECRUITER ====================
        public async Task<List<TestResponseDto>> GetMyTestsAsync(int recruiterId)
        {
            return await _db.Tests
                .Where(t => t.RecruiterId == recruiterId)
                .Select(t => new TestResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    TimeLimitMinutes = t.TimeLimitMinutes,
                    CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
                    RecruiterName = t.Recruiter.FullName
                })
                .ToListAsync();
        }

        // ==================== GET A TEST BY ID (WITH QUESTIONS) ====================
        public async Task<TestResponseDto> GetTestByIdAsync(int testId)
        {
            var test = await _db.Tests
                .Include(t => t.Recruiter)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
            {
                throw new KeyNotFoundException("Test not found.");
            }

            // Map to DTO (Notice we map Options to OptionResponseDto which hides IsCorrect)
            return new TestResponseDto
            {
                Id = test.Id,
                Title = test.Title,
                TimeLimitMinutes = test.TimeLimitMinutes,
                CreatedAt = test.CreatedAt ?? DateTime.UtcNow,
                RecruiterName = test.Recruiter.FullName,
                Questions = test.Questions.Select(q => new QuestionResponseDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Topic = q.Topic,
                    Difficulty = q.Difficulty ?? "Medium",
                    Options = q.Options.Select(o => new OptionResponseDto
                    {
                        Id = o.Id,
                        Text = o.Text
                        // IsCorrect is intentionally hidden!
                    }).ToList()
                }).ToList()
            };
        }
    }
}
