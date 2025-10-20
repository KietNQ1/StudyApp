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
    public class UserStreaksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserStreaksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserStreaks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserStreak>>> GetUserStreaks()
        {
            return await _context.UserStreaks.Include(us => us.User).ToListAsync();
        }

        // GET: api/UserStreaks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserStreak>> GetUserStreak(int id)
        {
            var userStreak = await _context.UserStreaks
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.Id == id);

            if (userStreak == null)
            {
                return NotFound();
            }

            return userStreak;
        }

        // POST: api/UserStreaks
        [HttpPost]
        public async Task<ActionResult<UserStreak>> PostUserStreak(UserStreak userStreak)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userStreak.UserId))
            {
                return BadRequest("Invalid User ID.");
            }

            _context.UserStreaks.Add(userStreak);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserStreak), new { id = userStreak.Id }, userStreak);
        }

        // PUT: api/UserStreaks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserStreak(int id, UserStreak userStreak)
        {
            if (id != userStreak.Id)
            {
                return BadRequest();
            }

            _context.Entry(userStreak).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserStreakExists(id))
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

        // DELETE: api/UserStreaks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserStreak(int id)
        {
            var userStreak = await _context.UserStreaks.FindAsync(id);
            if (userStreak == null)
            {
                return NotFound();
            }

            _context.UserStreaks.Remove(userStreak);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserStreakExists(int id)
        {
            return _context.UserStreaks.Any(e => e.Id == id);
        }
    }
}
