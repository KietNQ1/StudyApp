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
    public class ChatSessionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatSessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ChatSessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatSession>>> GetChatSessions()
        {
            return await _context.ChatSessions
                .Include(cs => cs.User)
                .Include(cs => cs.Course)
                .Include(cs => cs.Document)
                .ToListAsync();
        }

        // GET: api/ChatSessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatSession>> GetChatSession(int id)
        {
            var chatSession = await _context.ChatSessions
                .Include(cs => cs.User)
                .Include(cs => cs.Course)
                .Include(cs => cs.Document)
                .Include(cs => cs.ChatMessages)
                .ThenInclude(cm => cm.MessageCitations)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (chatSession == null)
            {
                return NotFound();
            }

            return chatSession;
        }

        // POST: api/ChatSessions
        [HttpPost]
        public async Task<ActionResult<ChatSession>> PostChatSession(ChatSession chatSession)
        {
            // Basic validation for UserId, CourseId, DocumentId
            if (!await _context.Users.AnyAsync(u => u.Id == chatSession.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            if (chatSession.CourseId.HasValue && !await _context.Courses.AnyAsync(c => c.Id == chatSession.CourseId.Value))
            {
                return BadRequest("Invalid Course ID.");
            }
            if (chatSession.DocumentId.HasValue && !await _context.Documents.AnyAsync(d => d.Id == chatSession.DocumentId.Value))
            {
                return BadRequest("Invalid Document ID.");
            }

            _context.ChatSessions.Add(chatSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChatSession), new { id = chatSession.Id }, chatSession);
        }

        // PUT: api/ChatSessions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChatSession(int id, ChatSession chatSession)
        {
            if (id != chatSession.Id)
            {
                return BadRequest();
            }

            _context.Entry(chatSession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatSessionExists(id))
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

        // DELETE: api/ChatSessions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatSession(int id)
        {
            var chatSession = await _context.ChatSessions.FindAsync(id);
            if (chatSession == null)
            {
                return NotFound();
            }

            _context.ChatSessions.Remove(chatSession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChatSessionExists(int id)
        {
            return _context.ChatSessions.Any(e => e.Id == id);
        }
    }
}
