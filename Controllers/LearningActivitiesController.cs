using Microsoft.AspNetCore.Authorization;
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
    [Authorize] // Protect this controller
    public class LearningActivitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LearningActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/LearningActivities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LearningActivity>>> GetLearningActivities()
        {
            return await _context.LearningActivities
                .Include(la => la.User)
                .Include(la => la.Course)
                .Include(la => la.Document)
                .ToListAsync();
        }

        // GET: api/LearningActivities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LearningActivity>> GetLearningActivity(int id)
        {
            var learningActivity = await _context.LearningActivities
                .Include(la => la.User)
                .Include(la => la.Course)
                .Include(la => la.Document)
                .FirstOrDefaultAsync(la => la.Id == id);

            if (learningActivity == null)
            {
                return NotFound();
            }

            return learningActivity;
        }

        // POST: api/LearningActivities - Record a new learning activity
        [HttpPost]
        public async Task<ActionResult<LearningActivity>> RecordLearningActivity(LearningActivity learningActivity)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == learningActivity.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            if (learningActivity.CourseId.HasValue && !await _context.Courses.AnyAsync(c => c.Id == learningActivity.CourseId.Value))
            {
                return BadRequest("Invalid Course ID.");
            }
            if (learningActivity.DocumentId.HasValue && !await _context.Documents.AnyAsync(d => d.Id == learningActivity.DocumentId.Value))
            {
                return BadRequest("Invalid Document ID.");
            }

            learningActivity.CreatedAt = DateTime.UtcNow;
            _context.LearningActivities.Add(learningActivity);
            await _context.SaveChangesAsync();

            // Optionally, update CourseProgress or DocumentProgress here based on activity type
            if (learningActivity.CourseId.HasValue)
            {
                var courseProgress = await _context.CourseProgresses
                    .FirstOrDefaultAsync(cp => cp.UserId == learningActivity.UserId && cp.CourseId == learningActivity.CourseId.Value);
                if (courseProgress != null)
                {
                    courseProgress.TimeSpentMinutes += learningActivity.DurationMinutes;
                    courseProgress.LastAccessedAt = DateTime.UtcNow;
                    courseProgress.Status = "in_progress"; // Ensure status is in_progress if active
                }
            }

            if (learningActivity.DocumentId.HasValue)
            {
                var documentProgress = await _context.DocumentProgresses
                    .FirstOrDefaultAsync(dp => dp.UserId == learningActivity.UserId && dp.DocumentId == learningActivity.DocumentId.Value);
                if (documentProgress != null)
                {
                    documentProgress.TimeSpentMinutes += learningActivity.DurationMinutes;
                    documentProgress.LastAccessedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(); // Save changes to progress entities

            return CreatedAtAction(nameof(GetLearningActivity), new { id = learningActivity.Id }, learningActivity);
        }

        // PUT: api/LearningActivities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLearningActivity(int id, LearningActivity learningActivity)
        {
            if (id != learningActivity.Id)
            {
                return BadRequest();
            }

            _context.Entry(learningActivity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LearningActivityExists(id))
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

        // DELETE: api/LearningActivities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearningActivity(int id)
        {
            var learningActivity = await _context.LearningActivities.FindAsync(id);
            if (learningActivity == null)
            {
                return NotFound();
            }

            _context.LearningActivities.Remove(learningActivity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LearningActivityExists(int id)
        {
            return _context.LearningActivities.Any(e => e.Id == id);
        }
    }
}
