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
    public class CourseProgressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseProgressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CourseProgresses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseProgress>>> GetCourseProgresses()
        {
            return await _context.CourseProgresses
                .Include(cp => cp.User)
                .Include(cp => cp.Course)
                .ToListAsync();
        }

        // GET: api/CourseProgresses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseProgress>> GetCourseProgress(int id)
        {
            var courseProgress = await _context.CourseProgresses
                .Include(cp => cp.User)
                .Include(cp => cp.Course)
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (courseProgress == null)
            {
                return NotFound();
            }

            return courseProgress;
        }

        // POST: api/CourseProgresses - Enroll a user in a course / Start tracking progress
        [HttpPost]
        public async Task<ActionResult<CourseProgress>> EnrollInCourse(CourseProgress courseProgress)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == courseProgress.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            if (!await _context.Courses.AnyAsync(c => c.Id == courseProgress.CourseId))
            {
                return BadRequest("Invalid Course ID.");
            }

            // Check if user is already enrolled
            var existingProgress = await _context.CourseProgresses
                .FirstOrDefaultAsync(cp => cp.UserId == courseProgress.UserId && cp.CourseId == courseProgress.CourseId);

            if (existingProgress != null)
            {
                return Conflict("User is already enrolled in this course.");
            }

            courseProgress.EnrollmentDate = DateTime.UtcNow;
            courseProgress.LastAccessedAt = DateTime.UtcNow;
            courseProgress.CompletionPercentage = 0;
            courseProgress.TimeSpentMinutes = 0;
            courseProgress.Status = "not_started"; // Initial status

            _context.CourseProgresses.Add(courseProgress);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourseProgress), new { id = courseProgress.Id }, courseProgress);
        }

        // PUT: api/CourseProgresses/5 - Update course progress (e.g., manually update completion or status)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourseProgress(int id, CourseProgress courseProgress)
        {
            if (id != courseProgress.Id)
            {
                return BadRequest();
            }

            _context.Entry(courseProgress).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseProgressExists(id))
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

        // POST: api/CourseProgresses/{id}/Complete - Mark a course as completed
        [HttpPost("{id}/Complete")]
        public async Task<IActionResult> CompleteCourse(int id)
        {
            var courseProgress = await _context.CourseProgresses
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (courseProgress == null)
            {
                return NotFound();
            }

            courseProgress.CompletionPercentage = 100;
            courseProgress.Status = "completed";
            courseProgress.LastAccessedAt = DateTime.UtcNow;
            // Potentially calculate total time spent here based on LearningActivities

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CourseProgresses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseProgress(int id)
        {
            var courseProgress = await _context.CourseProgresses.FindAsync(id);
            if (courseProgress == null)
            {
                return NotFound();
            }

            _context.CourseProgresses.Remove(courseProgress);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseProgressExists(int id)
        {
            return _context.CourseProgresses.Any(e => e.Id == id);
        }
    }
}
