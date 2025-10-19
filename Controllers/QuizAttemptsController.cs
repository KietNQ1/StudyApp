using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Models.DTOs;
using myapp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizAttemptsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly VertexAIService _vertexAIService;

        public QuizAttemptsController(ApplicationDbContext context, VertexAIService vertexAIService)
        {
            _context = context;
            _vertexAIService = vertexAIService;
        }
        
        [HttpPost]
        public async Task<ActionResult<QuizAttempt>> StartQuizAttempt([FromBody] StartQuizAttemptDto startQuizAttemptDto)
        {
            if (!await _context.Quizzes.AnyAsync(q => q.Id == startQuizAttemptDto.QuizId))
            {
                return BadRequest("Invalid Quiz ID.");
            }
            if (!await _context.Users.AnyAsync(u => u.Id == startQuizAttemptDto.UserId))
            {
                return BadRequest("Invalid User ID.");
            }

            var totalPoints = await _context.QuizQuestions
                                        .Where(qq => qq.QuizId == startQuizAttemptDto.QuizId)
                                        .SumAsync(qq => qq.PointsOverride > 0 ? qq.PointsOverride : qq.Question.Points);

            var quizAttempt = new QuizAttempt
            {
                QuizId = startQuizAttemptDto.QuizId,
                UserId = startQuizAttemptDto.UserId,
                StartedAt = DateTime.UtcNow,
                Status = "in_progress",
                Score = 0,
                Percentage = 0,
                TotalPoints = totalPoints
            };

            _context.QuizAttempts.Add(quizAttempt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuizAttempt), new { id = quizAttempt.Id }, quizAttempt);
        }

        // ... (rest of the controller is unchanged)

        [HttpPost("{id}/Submit")]
        public async Task<ActionResult<QuizAttempt>> SubmitQuizAttempt(int id, [FromBody] List<QuizAnswer> answers)
        {
            var quizAttempt = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.QuizAnswers)
                .FirstOrDefaultAsync(qa => qa.Id == id);

            if (quizAttempt == null || quizAttempt.Status != "in_progress")
            {
                return BadRequest("Quiz attempt not found or not in progress.");
            }

            double totalEarnedPoints = 0;
            foreach (var submittedAnswer in answers)
            {
                var question = await _context.Questions.Include(q => q.QuestionOptions).FirstOrDefaultAsync(q => q.Id == submittedAnswer.QuestionId);
                if (question == null) continue;

                var quizQuestion = await _context.QuizQuestions
                    .FirstOrDefaultAsync(qq => qq.QuizId == quizAttempt.QuizId && qq.QuestionId == question.Id);
                
                var questionPoints = quizQuestion?.PointsOverride > 0 ? quizQuestion.PointsOverride : question.Points;

                submittedAnswer.AttemptId = id;
                submittedAnswer.AnsweredAt = DateTime.UtcNow;

                if (question.QuestionType == "multiple_choice")
                {
                    var correctOption = question.QuestionOptions.FirstOrDefault(qo => qo.IsCorrect);
                    if (submittedAnswer.SelectedOptionId.HasValue && submittedAnswer.SelectedOptionId.Value == correctOption?.Id)
                    {
                        submittedAnswer.IsCorrect = true;
                        submittedAnswer.PointsEarned = questionPoints;
                    }
                }
                else if (question.QuestionType == "short_answer" || question.QuestionType == "essay")
                {
                    if (!string.IsNullOrEmpty(submittedAnswer.TextAnswer) && !string.IsNullOrEmpty(question.Explanation))
                    {
                        try
                        {
                            var gradingPrompt = $"Grade the following student answer for the question: \"{question.QuestionText}\".\nCorrect Answer/Explanation: \"{question.Explanation}\".\nStudent Answer: \"{submittedAnswer.TextAnswer}\".\nProvide a score (0-100) and a brief feedback. Format as JSON: {{ \"score\": int, \"feedback\": \"string\" }}";
                            var aiGradingResponseJson = await _vertexAIService.PredictTextAsync(gradingPrompt, 0.2);
                            
                            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var aiGrade = JsonSerializer.Deserialize<AIGradeResponse>(aiGradingResponseJson, jsonOptions);

                            if (aiGrade != null)
                            {
                                submittedAnswer.PointsEarned = (questionPoints * aiGrade.Score / 100.0);
                                submittedAnswer.AiFeedback = aiGrade.Feedback;
                                submittedAnswer.IsCorrect = aiGrade.Score >= 70;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error grading with AI: {ex.Message}");
                            submittedAnswer.AiFeedback = "Could not grade with AI due to an error.";
                        }
                    }
                }
                
                totalEarnedPoints += submittedAnswer.PointsEarned;
                quizAttempt.QuizAnswers.Add(submittedAnswer);
            }

            quizAttempt.Score = totalEarnedPoints;
            quizAttempt.Percentage = (quizAttempt.TotalPoints > 0) ? (totalEarnedPoints / quizAttempt.TotalPoints * 100) : 0;
            quizAttempt.SubmittedAt = DateTime.UtcNow;
            quizAttempt.Status = "completed";
            quizAttempt.TimeSpentSeconds = (int)(quizAttempt.SubmittedAt.Value - quizAttempt.StartedAt).TotalSeconds;

            await _context.SaveChangesAsync();

            return Ok(quizAttempt);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAttempt>>> GetQuizAttempts()
        {
            return await _context.QuizAttempts.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizAttempt>> GetQuizAttempt(int id)
        {
            var quizAttempt = await _context.QuizAttempts.FindAsync(id);
            if (quizAttempt == null)
            {
                return NotFound();
            }
            return quizAttempt;
        }
    }

    public class AIGradeResponse
    {
        public int Score { get; set; }
        public string? Feedback { get; set; }
    }
}
