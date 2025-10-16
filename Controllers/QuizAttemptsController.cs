using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; // Add this using directive
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

        // GET: api/QuizAttempts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAttempt>>> GetQuizAttempts()
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .ToListAsync();
        }

        // GET: api/QuizAttempts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizAttempt>> GetQuizAttempt(int id)
        {
            var quizAttempt = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Include(qa => qa.QuizAnswers)
                    .ThenInclude(qans => qans.Question)
                        .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(qa => qa.Id == id);

            if (quizAttempt == null)
            {
                return NotFound();
            }

            return quizAttempt;
        }

        // POST: api/QuizAttempts - Start a new quiz attempt
        [HttpPost]
        public async Task<ActionResult<QuizAttempt>> StartQuizAttempt(QuizAttempt quizAttempt)
        {
            if (!await _context.Quizzes.AnyAsync(q => q.Id == quizAttempt.QuizId))
            {
                return BadRequest("Invalid Quiz ID.");
            }
            if (!await _context.Users.AnyAsync(u => u.Id == quizAttempt.UserId))
            {
                return BadRequest("Invalid User ID.");
            }

            quizAttempt.StartedAt = DateTime.UtcNow;
            quizAttempt.Status = "in_progress";
            quizAttempt.Score = 0;
            quizAttempt.Percentage = 0;
            quizAttempt.TotalPoints = await _context.QuizQuestions
                                            .Where(qq => qq.QuizId == quizAttempt.QuizId)
                                            .SumAsync(qq => qq.PointsOverride > 0 ? qq.PointsOverride : qq.Question.Points);

            _context.QuizAttempts.Add(quizAttempt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuizAttempt), new { id = quizAttempt.Id }, quizAttempt);
        }

        // POST: api/QuizAttempts/{id}/Submit - Submit a quiz attempt and get graded
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

            // Calculate score and provide AI feedback for essay/short_answer questions
            double totalEarnedPoints = 0;
            foreach (var submittedAnswer in answers)
            {
                var question = await _context.Questions.Include(q => q.QuestionOptions).FirstOrDefaultAsync(q => q.Id == submittedAnswer.QuestionId);
                if (question == null)
                {
                    continue; // Skip invalid questions
                }

                var quizQuestion = await _context.QuizQuestions
                    .FirstOrDefaultAsync(qq => qq.QuizId == quizAttempt.QuizId && qq.QuestionId == question.Id);
                
                var questionPoints = quizQuestion?.PointsOverride > 0 ? quizQuestion.PointsOverride : question.Points;

                submittedAnswer.AttemptId = id;
                submittedAnswer.QuestionId = question.Id;
                submittedAnswer.AnsweredAt = DateTime.UtcNow;

                if (question.QuestionType == "multiple_choice")
                {
                    var correctOption = question.QuestionOptions.FirstOrDefault(qo => qo.IsCorrect);
                    if (submittedAnswer.SelectedOptionId.HasValue && submittedAnswer.SelectedOptionId.Value == correctOption?.Id)
                    {
                        submittedAnswer.IsCorrect = true;
                        submittedAnswer.PointsEarned = questionPoints;
                    }
                    else
                    {
                        submittedAnswer.IsCorrect = false;
                        submittedAnswer.PointsEarned = 0;
                    }
                }
                else if (question.QuestionType == "short_answer" || question.QuestionType == "essay")
                {
                    // Use AI to grade short answer/essay questions
                    if (!string.IsNullOrEmpty(submittedAnswer.TextAnswer) && !string.IsNullOrEmpty(question.Explanation))
                    {
                        try
                        {
                            // The prompt for grading can be more sophisticated
                            var gradingPrompt = $"Grade the following student answer for the question: \"{question.QuestionText}\".\nCorrect Answer/Explanation: \"{question.Explanation}\".\nStudent Answer: \"{submittedAnswer.TextAnswer}\".\nProvide a score (0-100) and a brief feedback. Format as JSON: {{ \"score\": int, \"feedback\": \"string\" }}";
                            var aiGradingResponseJson = await _vertexAIService.PredictTextAsync(gradingPrompt, 0.2); // Lower temperature for factual grading
                            
                            // Attempt to parse AI response
                            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var aiGrade = JsonSerializer.Deserialize<AIGradeResponse>(aiGradingResponseJson, jsonOptions);

                            if (aiGrade != null)
                            {
                                // Scale AI's 0-100 score to actual question points
                                submittedAnswer.PointsEarned = (questionPoints * aiGrade.Score / 100.0);
                                submittedAnswer.AiFeedback = aiGrade.Feedback;
                                submittedAnswer.IsCorrect = aiGrade.Score >= 70; // Example threshold
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error grading with AI: {ex.Message}");
                            submittedAnswer.AiFeedback = "Could not grade with AI due to an error.";
                            submittedAnswer.PointsEarned = 0;
                            submittedAnswer.IsCorrect = false;
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

        // PUT: api/QuizAttempts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuizAttempt(int id, QuizAttempt quizAttempt)
        {
            if (id != quizAttempt.Id)
            {
                return BadRequest();
            }

            _context.Entry(quizAttempt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizAttemptExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/QuizAttempts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizAttempt(int id)
        {
            var quizAttempt = await _context.QuizAttempts.FindAsync(id);
            if (quizAttempt == null)
            {
                return NotFound();
            }

            _context.QuizAttempts.Remove(quizAttempt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizAttemptExists(int id)
        {
            return _context.QuizAttempts.Any(e => e.Id == id);
        }
    }

    public class AIGradeResponse
    {
        public int Score { get; set; }
        public string? Feedback { get; set; } // Made nullable
    }
}
