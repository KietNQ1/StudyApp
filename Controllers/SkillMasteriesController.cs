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
    public class SkillMasteriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SkillMasteriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SkillMasteries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillMastery>>> GetSkillMasteries()
        {
            return await _context.SkillMasteries
                .Include(sm => sm.User)
                .Include(sm => sm.Course)
                .Include(sm => sm.Topic)
                .ToListAsync();
        }

        // GET: api/SkillMasteries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SkillMastery>> GetSkillMastery(int id)
        {
            var skillMastery = await _context.SkillMasteries
                .Include(sm => sm.User)
                .Include(sm => sm.Course)
                .Include(sm => sm.Topic)
                .FirstOrDefaultAsync(sm => sm.Id == id);

            if (skillMastery == null)
            {
                return NotFound();
            }

            return skillMastery;
        }

        // POST: api/SkillMasteries
        [HttpPost]
        public async Task<ActionResult<SkillMastery>> PostSkillMastery(SkillMastery skillMastery)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == skillMastery.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            if (!await _context.Courses.AnyAsync(c => c.Id == skillMastery.CourseId))
            {
                return BadRequest("Invalid Course ID.");
            }
            if (!await _context.Topics.AnyAsync(t => t.Id == skillMastery.TopicId))
            {
                return BadRequest("Invalid Topic ID.");
            }

            _context.SkillMasteries.Add(skillMastery);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSkillMastery), new { id = skillMastery.Id }, skillMastery);
        }

        // PUT: api/SkillMasteries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSkillMastery(int id, SkillMastery skillMastery)
        {
            if (id != skillMastery.Id)
            {
                return BadRequest();
            }

            _context.Entry(skillMastery).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkillMasteryExists(id))
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

        // DELETE: api/SkillMasteries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkillMastery(int id)
        {
            var skillMastery = await _context.SkillMasteries.FindAsync(id);
            if (skillMastery == null)
            {
                return NotFound();
            }

            _context.SkillMasteries.Remove(skillMastery);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SkillMasteryExists(int id)
        {
            return _context.SkillMasteries.Any(e => e.Id == id);
        }
    }
}
