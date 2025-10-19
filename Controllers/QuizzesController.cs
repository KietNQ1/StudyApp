using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    public class GenerateQuizRequest
    {
        public int CourseId { get; set; }
        public int? TopicId { get; set; }
        public int? DocumentId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public int NumberOfQuestions { get; set; } = 5;
        public string QuestionType { get; set; } = "multiple_choice";
        public int CreatedByUserId { get; set; }
    }

    public class GeneratedQuestionDto
    {
        public required string QuestionText { get; set; }
        public List<string>? Options { get; set; }
        public required string CorrectAnswer { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly VertexAIService _vertexAIService;

        public QuizzesController(ApplicationDbContext context, VertexAIService vertexAIService)
        {
            _context = context;
            _vertexAIService = vertexAIService;
        }

        private string ExtractJsonFromMarkdown(string text)
        {
            var match = Regex.Match(text, @"```json\s*([\s\S]*?)\s*```");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return text;
        }

        [HttpPost("Generate")]
        public async Task<ActionResult<Quiz>> GenerateQuiz(GenerateQuizRequest request)
        {
            string contentForAI;
            if (request.DocumentId.HasValue)
            {
                var document = await _context.Documents.FindAsync(request.DocumentId.Value);
                if (document == null || string.IsNullOrEmpty(document.ExtractedText))
                {
                    return BadRequest("Document not found or has no extracted text.");
                }
                contentForAI = document.ExtractedText;
            }
            else
            {
                return BadRequest("DocumentId must be provided for AI generation.");
            }

            string rawAiResponse;
            try
            {
                rawAiResponse = await _vertexAIService.GenerateQuizQuestions(
                    contentForAI,
                    request.Title,
                    request.NumberOfQuestions,
                    request.QuestionType
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating quiz questions: {ex.Message}");
            }

            List<GeneratedQuestionDto>? generatedQuestions;
            try
            {
                var cleanJson = ExtractJsonFromMarkdown(rawAiResponse);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(cleanJson, options);
                if (generatedQuestions == null || !generatedQuestions.Any())
                {
                    return BadRequest("AI did not return valid questions.");
                }
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"AI response was not in the expected JSON format: {ex.Message}. Raw Response: {rawAiResponse}");
            }

            var newQuiz = new Quiz
            {
                CourseId = request.CourseId,
                TopicId = request.TopicId,
                CreatedBy = request.CreatedByUserId,
                Title = request.Title,
                Description = request.Description ?? "AI-generated quiz",
            };
            _context.Quizzes.Add(newQuiz);
            await _context.SaveChangesAsync();

            foreach (var qDto in generatedQuestions)
            {
                var question = new Question
                {
                    CourseId = request.CourseId,
                    TopicId = request.TopicId,
                    DocumentId = request.DocumentId,
                    QuestionType = request.QuestionType,
                    QuestionText = qDto.QuestionText,
                    GeneratedByAi = true,
                };
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                if (request.QuestionType == "multiple_choice" && qDto.Options != null)
                {
                    foreach (var optionText in qDto.Options)
                    {
                        _context.QuestionOptions.Add(new QuestionOption
                        {
                            QuestionId = question.Id,
                            OptionText = optionText,
                            IsCorrect = (optionText == qDto.CorrectAnswer),
                        });
                    }
                }
                _context.QuizQuestions.Add(new QuizQuestion
                {
                    QuizId = newQuiz.Id,
                    QuestionId = question.Id,
                });
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetQuiz), new { id = newQuiz.Id }, newQuiz);
        }

        // Other methods (GET, PUT, DELETE) are unchanged...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuizzes()
        {
            return await _context.Quizzes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }
            return quiz;
        }
    }
}
