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
    public class UserPointsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserPointsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserPoints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserPoint>>> GetUserPoints()
        {
            return await _context.UserPoints
                .Include(up => up.User)
                .Include(up => up.Course)
                .ToListAsync();
        }

        // GET: api/UserPoints/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserPoint>> GetUserPoint(int id)
        {
            var userPoint = await _context.UserPoints
                .Include(up => up.User)
                .Include(up => up.Course)
                .FirstOrDefaultAsync(up => up.Id == id);

            if (userPoint == null)
            {
                return NotFound();
            }

            return userPoint;
        }

        // POST: api/UserPoints
        [HttpPost]
        public async Task<ActionResult<UserPoint>> PostUserPoint(UserPoint userPoint)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userPoint.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            if (userPoint.CourseId.HasValue && !await _context.Courses.AnyAsync(c => c.Id == userPoint.CourseId.Value))
            {
                return BadRequest("Invalid Course ID.");
            }

            _context.UserPoints.Add(userPoint);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserPoint), new { id = userPoint.Id }, userPoint);
        }

        // PUT: api/UserPoints/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserPoint(int id, UserPoint userPoint)
        {
            if (id != userPoint.Id)
            {
                return BadRequest();
            }

            _context.Entry(userPoint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserPointExists(id))
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

        // DELETE: api/UserPoints/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserPoint(int id)
        {
            var userPoint = await _context.UserPoints.FindAsync(id);
            if (userPoint == null)
            {
                return NotFound();
            }

            _context.UserPoints.Remove(userPoint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserPointExists(int id)
        {
            return _context.UserPoints.Any(e => e.Id == id);
        }
    }
}
