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
    public class DocumentProgressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DocumentProgressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DocumentProgresses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentProgress>>> GetDocumentProgresses()
        {
            return await _context.DocumentProgresses
                .Include(dp => dp.User)
                .Include(dp => dp.Document)
                .ToListAsync();
        }

        // GET: api/DocumentProgresses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentProgress>> GetDocumentProgress(int id)
        {
            var documentProgress = await _context.DocumentProgresses
                .Include(dp => dp.User)
                .Include(dp => dp.Document)
                .FirstOrDefaultAsync(dp => dp.Id == id);

            if (documentProgress == null)
            {
                return NotFound();
            }

            return documentProgress;
        }

        // POST: api/DocumentProgresses - Start tracking progress for a document
        [HttpPost]
        public async Task<ActionResult<DocumentProgress>> StartDocumentProgress(DocumentProgress documentProgress)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == documentProgress.UserId))
            {
                return BadRequest("Invalid User ID.");
            }
            var document = await _context.Documents.FindAsync(documentProgress.DocumentId);
            if (document == null)
            {
                return BadRequest("Invalid Document ID.");
            }

            // Check if progress already exists
            var existingProgress = await _context.DocumentProgresses
                .FirstOrDefaultAsync(dp => dp.UserId == documentProgress.UserId && dp.DocumentId == documentProgress.DocumentId);
            
            if (existingProgress != null)
            {
                return Conflict("Document progress already exists for this user.");
            }

            documentProgress.CurrentPage = 1;
            documentProgress.TotalPages = document.PageCount;
            documentProgress.CompletionPercentage = 0;
            documentProgress.TimeSpentMinutes = 0;
            documentProgress.LastAccessedAt = DateTime.UtcNow;
            documentProgress.IsCompleted = false;

            _context.DocumentProgresses.Add(documentProgress);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocumentProgress), new { id = documentProgress.Id }, documentProgress);
        }

        // PUT: api/DocumentProgresses/5 - Update reading progress
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocumentReadingProgress(int id, [FromBody] DocumentReadingProgressUpdateDto updateDto)
        {
            var documentProgress = await _context.DocumentProgresses.FindAsync(id);
            if (documentProgress == null)
            {
                return NotFound();
            }

            documentProgress.CurrentPage = updateDto.CurrentPage;
            documentProgress.TimeSpentMinutes += updateDto.TimeSpentMinutes;
            documentProgress.LastAccessedAt = DateTime.UtcNow;

            if (documentProgress.TotalPages > 0)
            {
                documentProgress.CompletionPercentage = (double)documentProgress.CurrentPage / documentProgress.TotalPages * 100;
            }

            if (documentProgress.CurrentPage >= documentProgress.TotalPages)
            {
                documentProgress.IsCompleted = true;
                documentProgress.CompletionPercentage = 100;
            }

            _context.Entry(documentProgress).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentProgressExists(id))
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

        // DELETE: api/DocumentProgresses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumentProgress(int id)
        {
            var documentProgress = await _context.DocumentProgresses.FindAsync(id);
            if (documentProgress == null)
            {
                return NotFound();
            }

            _context.DocumentProgresses.Remove(documentProgress);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DocumentProgressExists(int id)
        {
            return _context.DocumentProgresses.Any(e => e.Id == id);
        }
    }

    public class DocumentReadingProgressUpdateDto
    {
        public int CurrentPage { get; set; }
        public int TimeSpentMinutes { get; set; }
    }
}
