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
    public class UserAchievementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserAchievementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserAchievements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAchievement>>> GetUserAchievements()
        {
            return await _context.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .ToListAsync();
        }

        // GET: api/UserAchievements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAchievement>> GetUserAchievement(int id)
        {
            var userAchievement = await _context.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .FirstOrDefaultAsync(ua => ua.Id == id);

            if (userAchievement == null)
            {
                return NotFound();
            }

            return userAchievement;
        }

        // POST: api/UserAchievements
        [HttpPost]
        public async Task<ActionResult<UserAchievement>> PostUserAchievement(UserAchievement userAchievement)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userAchievement.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            if (!await _context.Achievements.AnyAsync(a => a.Id == userAchievement.AchievementId))
            {
                return BadRequest("Invalid Achievement ID.");
            }

            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserAchievement), new { id = userAchievement.Id }, userAchievement);
        }

        // PUT: api/UserAchievements/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAchievement(int id, UserAchievement userAchievement)
        {
            if (id != userAchievement.Id)
            {
                return BadRequest();
            }

            _context.Entry(userAchievement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAchievementExists(id))
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

        // DELETE: api/UserAchievements/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAchievement(int id)
        {
            var userAchievement = await _context.UserAchievements.FindAsync(id);
            if (userAchievement == null)
            {
                return NotFound();
            }

            _context.UserAchievements.Remove(userAchievement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserAchievementExists(int id)
        {
            return _context.UserAchievements.Any(e => e.Id == id);
        }
    }
}
