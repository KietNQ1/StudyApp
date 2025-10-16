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
    public class QuizAnswersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuizAnswersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/QuizAnswers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAnswer>>> GetQuizAnswers()
        {
            return await _context.QuizAnswers
                .Include(qa => qa.QuizAttempt)
                .Include(qa => qa.Question)
                .Include(qa => qa.SelectedOption)
                .ToListAsync();
        }

        // GET: api/QuizAnswers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizAnswer>> GetQuizAnswer(int id)
        {
            var quizAnswer = await _context.QuizAnswers
                .Include(qa => qa.QuizAttempt)
                .Include(qa => qa.Question)
                .Include(qa => qa.SelectedOption)
                .FirstOrDefaultAsync(qa => qa.Id == id);

            if (quizAnswer == null)
            {
                return NotFound();
            }

            return quizAnswer;
        }

        // POST: api/QuizAnswers
        [HttpPost]
        public async Task<ActionResult<QuizAnswer>> PostQuizAnswer(QuizAnswer quizAnswer)
        {
            if (!await _context.QuizAttempts.AnyAsync(qa => qa.Id == quizAnswer.AttemptId))
            {
                return BadRequest("Invalid Quiz Attempt ID.");
            }
            if (!await _context.Questions.AnyAsync(q => q.Id == quizAnswer.QuestionId))
            {
                return BadRequest("Invalid Question ID.");
            }
            if (quizAnswer.SelectedOptionId.HasValue && !await _context.QuestionOptions.AnyAsync(qo => qo.Id == quizAnswer.SelectedOptionId.Value))
            {
                return BadRequest("Invalid Question Option ID.");
            }

            quizAnswer.AnsweredAt = DateTime.UtcNow;
            _context.QuizAnswers.Add(quizAnswer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuizAnswer), new { id = quizAnswer.Id }, quizAnswer);
        }

        // PUT: api/QuizAnswers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuizAnswer(int id, QuizAnswer quizAnswer)
        {
            if (id != quizAnswer.Id)
            {
                return BadRequest();
            }

            _context.Entry(quizAnswer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizAnswerExists(id))
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

        // DELETE: api/QuizAnswers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizAnswer(int id)
        {
            var quizAnswer = await _context.QuizAnswers.FindAsync(id);
            if (quizAnswer == null)
            {
                return NotFound();
            }

            _context.QuizAnswers.Remove(quizAnswer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizAnswerExists(int id)
        {
            return _context.QuizAnswers.Any(e => e.Id == id);
        }
    }
}
