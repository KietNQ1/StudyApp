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
    public class AIUsageLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AIUsageLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AIUsageLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AIUsageLog>>> GetAIUsageLogs()
        {
            return await _context.AIUsageLogs.Include(aul => aul.User).ToListAsync();
        }

        // GET: api/AIUsageLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AIUsageLog>> GetAIUsageLog(int id)
        {
            var aiUsageLog = await _context.AIUsageLogs
                .Include(aul => aul.User)
                .FirstOrDefaultAsync(aul => aul.Id == id);

            if (aiUsageLog == null)
            {
                return NotFound();
            }

            return aiUsageLog;
        }

        // POST: api/AIUsageLogs
        [HttpPost]
        public async Task<ActionResult<AIUsageLog>> PostAIUsageLog(AIUsageLog aiUsageLog)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == aiUsageLog.UserId))
            {
                return BadRequest("Invalid User ID.");
            }

            _context.AIUsageLogs.Add(aiUsageLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAIUsageLog), new { id = aiUsageLog.Id }, aiUsageLog);
        }

        // PUT: api/AIUsageLogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAIUsageLog(int id, AIUsageLog aiUsageLog)
        {
            if (id != aiUsageLog.Id)
            {
                return BadRequest();
            }

            _context.Entry(aiUsageLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AIUsageLogExists(id))
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

        // DELETE: api/AIUsageLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAIUsageLog(int id)
        {
            var aiUsageLog = await _context.AIUsageLogs.FindAsync(id);
            if (aiUsageLog == null)
            {
                return NotFound();
            }

            _context.AIUsageLogs.Remove(aiUsageLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AIUsageLogExists(int id)
        {
            return _context.AIUsageLogs.Any(e => e.Id == id);
        }
    }
}
