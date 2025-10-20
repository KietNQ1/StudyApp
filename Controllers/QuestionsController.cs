using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protect this controller
    public class QuestionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Document)
                .Include(q => q.QuestionOptions)
                .ToListAsync();
        }

        // GET: api/Questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Document)
                .Include(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        // POST: api/Questions
        [HttpPost]
        public async Task<ActionResult<Question>> PostQuestion(Question question)
        {
            // Basic validation for foreign keys
            if (question.CourseId.HasValue && !await _context.Courses.AnyAsync(c => c.Id == question.CourseId.Value))
            {
                return BadRequest("Invalid Course ID.");
            }
            if (question.TopicId.HasValue && !await _context.Topics.AnyAsync(t => t.Id == question.TopicId.Value))
            {
                return BadRequest("Invalid Topic ID.");
            }
            if (question.DocumentId.HasValue && !await _context.Documents.AnyAsync(d => d.Id == question.DocumentId.Value))
            {
                return BadRequest("Invalid Document ID.");
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
        }

        // PUT: api/Questions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuestion(int id, Question question)
        {
            if (id != question.Id)
            {
                return BadRequest();
            }

            _context.Entry(question).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
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

        // DELETE: api/Questions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
