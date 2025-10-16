using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        public string QuestionType { get; set; } = "multiple_choice"; // multiple_choice, short_answer
        public int CreatedByUserId { get; set; }
    }

    public class GeneratedQuestionDto
    {
        public required string QuestionText { get; set; }
        public List<string>? Options { get; set; }
        public required string CorrectAnswer { get; set; } // For multiple choice, this might be the text of the correct option
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

        // GET: api/Quizzes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuizzes()
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Creator)
                .ToListAsync();
        }

        // GET: api/Quizzes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Creator)
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

        // POST: api/Quizzes - Create a quiz manually
        [HttpPost]
        public async Task<ActionResult<Quiz>> PostQuiz(Quiz quiz)
        {
            if (!await _context.Courses.AnyAsync(c => c.Id == quiz.CourseId))
            {
                return BadRequest("Invalid Course ID.");
            }
            if (quiz.TopicId.HasValue && !await _context.Topics.AnyAsync(t => t.Id == quiz.TopicId.Value))
            {
                return BadRequest("Invalid Topic ID.");
            }
            if (!await _context.Users.AnyAsync(u => u.Id == quiz.CreatedBy))
            {
                return BadRequest("Invalid Creator User ID.");
            }

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
        }

        // POST: api/Quizzes/Generate
        [HttpPost("Generate")]
        public async Task<ActionResult<Quiz>> GenerateQuiz(GenerateQuizRequest request)
        {
            // 1. Validate request and retrieve content
            string contentForAI = null;
            if (request.DocumentId.HasValue)
            {
                var document = await _context.Documents.FindAsync(request.DocumentId.Value);
                if (document == null || string.IsNullOrEmpty(document.ExtractedText))
                {
                    return BadRequest("Document not found or has no extracted text.");
                }
                contentForAI = document.ExtractedText;
            }
            else if (request.TopicId.HasValue)
            {
                var topic = await _context.Topics.FindAsync(request.TopicId.Value);
                if (topic == null || string.IsNullOrEmpty(topic.Description))
                {
                    return BadRequest("Topic not found or has no description.");
                }
                contentForAI = topic.Description; // Or combine with related documents
            }
            else
            {
                return BadRequest("Either DocumentId or TopicId must be provided for AI generation.");
            }

            if (string.IsNullOrEmpty(contentForAI))
            {
                return BadRequest("Content for AI generation is empty.");
            }

            // 2. Call AI Service to generate questions
            string aiResponseJson;
            try
            {
                aiResponseJson = await _vertexAIService.GenerateQuizQuestions(
                    contentForAI,
                    request.Title, // Use quiz title as topic for AI
                    request.NumberOfQuestions,
                    request.QuestionType
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating quiz with AI: {ex.Message}");
                return StatusCode(500, $"Error generating quiz questions: {ex.Message}");
            }

            List<GeneratedQuestionDto>? generatedQuestions;
            try
            {
                // Ensure AI provides valid JSON
                generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(aiResponseJson);
                if (generatedQuestions == null || !generatedQuestions.Any())
                {
                    return BadRequest("AI did not return valid questions.");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing AI response: {ex.Message}. Raw AI Response: {aiResponseJson}");
                return StatusCode(500, "AI response was not in the expected JSON format.");
            }

            // 3. Create Quiz and Questions in DB
            var newQuiz = new Quiz
            {
                CourseId = request.CourseId,
                TopicId = request.TopicId,
                CreatedBy = request.CreatedByUserId,
                Title = request.Title,
                Description = request.Description ?? "AI-generated quiz",
                TimeLimitMinutes = 30, // Default or configurable
                PassingScore = 70,    // Default or configurable
                ShuffleQuestions = true,
                ShuffleOptions = true,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow
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
                    DifficultyLevel = "medium", // Default or AI-determined
                    Points = 10,                 // Default or AI-determined
                    GeneratedByAi = true,
                    CreatedAt = DateTime.UtcNow
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
                            OrderIndex = qDto.Options.IndexOf(optionText)
                        });
                    }
                }
                else if (request.QuestionType == "short_answer")
                {
                    // For short answer, the 'correctAnswer' might be stored in the Explanation for grading purposes
                    question.Explanation = qDto.CorrectAnswer; 
                }

                _context.QuizQuestions.Add(new QuizQuestion
                {
                    QuizId = newQuiz.Id,
                    QuestionId = question.Id,
                    OrderIndex = generatedQuestions.IndexOf(qDto) // Simple ordering
                });
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetQuiz), new { id = newQuiz.Id }, newQuiz);
        }

        // PUT: api/Quizzes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return BadRequest();
            }

            _context.Entry(quiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
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

        // DELETE: api/Quizzes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
}
