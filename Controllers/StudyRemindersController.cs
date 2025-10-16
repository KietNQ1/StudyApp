using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyRemindersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudyRemindersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/StudyReminders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudyReminder>>> GetStudyReminders()
        {
            return await _context.StudyReminders
                .Include(sr => sr.User)
                .Include(sr => sr.Course)
                .ToListAsync();
        }

        // GET: api/StudyReminders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudyReminder>> GetStudyReminder(int id)
        {
            var studyReminder = await _context.StudyReminders
                .Include(sr => sr.User)
                .Include(sr => sr.Course)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (studyReminder == null)
            {
                return NotFound();
            }

            return studyReminder;
        }

        // POST: api/StudyReminders - Create a new study reminder
        [HttpPost]
        public async Task<ActionResult<StudyReminder>> CreateStudyReminder(StudyReminder studyReminder)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == studyReminder.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            if (studyReminder.CourseId.HasValue && !await _context.Courses.AnyAsync(c => c.Id == studyReminder.CourseId.Value))
            {
                return BadRequest("Invalid Course ID.");
            }

            studyReminder.IsActive = true; // New reminders are active by default
            // DaysOfWeek can be a JSON string, ensure proper handling in client/logic

            _context.StudyReminders.Add(studyReminder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudyReminder), new { id = studyReminder.Id }, studyReminder);
        }

        // PUT: api/StudyReminders/5 - Update an existing study reminder
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudyReminder(int id, StudyReminder studyReminder)
        {
            if (id != studyReminder.Id)
            {
                return BadRequest();
            }

            _context.Entry(studyReminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyReminderExists(id))
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

        // POST: api/StudyReminders/{id}/ToggleActive - Toggle active status of a reminder
        [HttpPost("{id}/ToggleActive")]
        public async Task<IActionResult> ToggleStudyReminderActiveStatus(int id)
        {
            var studyReminder = await _context.StudyReminders.FindAsync(id);

            if (studyReminder == null)
            {
                return NotFound();
            }

            studyReminder.IsActive = !studyReminder.IsActive;
            _context.Entry(studyReminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyReminderExists(id))
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

        // DELETE: api/StudyReminders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyReminder(int id)
        {
            var studyReminder = await _context.StudyReminders.FindAsync(id);
            if (studyReminder == null)
            {
                return NotFound();
            }

            _context.StudyReminders.Remove(studyReminder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudyReminderExists(int id)
        {
            return _context.StudyReminders.Any(e => e.Id == id);
        }
    }
}
