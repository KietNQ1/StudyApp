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
    public class AICourseSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AICourseSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AICourseSettings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AICourseSetting>>> GetAICourseSettings()
        {
            return await _context.AICourseSettings
                .Include(acs => acs.Course)
                .ToListAsync();
        }

        // GET: api/AICourseSettings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AICourseSetting>> GetAICourseSetting(int id)
        {
            var aiCourseSetting = await _context.AICourseSettings
                .Include(acs => acs.Course)
                .FirstOrDefaultAsync(acs => acs.Id == id);

            if (aiCourseSetting == null)
            {
                return NotFound();
            }

            return aiCourseSetting;
        }

        // POST: api/AICourseSettings
        [HttpPost]
        public async Task<ActionResult<AICourseSetting>> PostAICourseSetting(AICourseSetting aiCourseSetting)
        {
            if (!await _context.Courses.AnyAsync(c => c.Id == aiCourseSetting.CourseId))
            {
                return BadRequest("Invalid Course ID.");
            }

            _context.AICourseSettings.Add(aiCourseSetting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAICourseSetting), new { id = aiCourseSetting.Id }, aiCourseSetting);
        }

        // PUT: api/AICourseSettings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAICourseSetting(int id, AICourseSetting aiCourseSetting)
        {
            if (id != aiCourseSetting.Id)
            {
                return BadRequest();
            }

            _context.Entry(aiCourseSetting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AICourseSettingExists(id))
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

        // DELETE: api/AICourseSettings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAICourseSetting(int id)
        {
            var aiCourseSetting = await _context.AICourseSettings.FindAsync(id);
            if (aiCourseSetting == null)
            {
                return NotFound();
            }

            _context.AICourseSettings.Remove(aiCourseSetting);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AICourseSettingExists(int id)
        {
            return _context.AICourseSettings.Any(e => e.Id == id);
        }
    }
}
