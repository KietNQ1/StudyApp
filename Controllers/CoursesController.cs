using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Add this for User.FindFirst
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Courses - Get courses for the current user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var courses = await _context.Courses
                .Where(c => c.UserId == int.Parse(userId))
                .Include(c => c.User)
                .ToListAsync();
            
            return courses;
        }

        // GET: api/Courses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var course = await _context.Courses
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Include(c => c.Topics)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == int.Parse(userId));

            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        // POST: api/Courses
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(CreateCourseDto createCourseDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var course = new Course
            {
                UserId = int.Parse(userId), // Set UserId from the authenticated user's token
                Title = createCourseDto.Title,
                Description = createCourseDto.Description,
                IsPublic = false,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        // ... (PUT and DELETE should also be updated to check for ownership)
        
        // PUT: api/Courses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, Course course)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || course.UserId != int.Parse(userId))
            {
                return Forbid(); // User does not own this course
            }

            if (id != course.Id)
            {
                return BadRequest();
            }

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
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

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var course = await _context.Courses.FindAsync(id);
            
            if (course == null)
            {
                return NotFound();
            }

            if (userId == null || course.UserId != int.Parse(userId))
            {
                return Forbid(); // User does not own this course
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
