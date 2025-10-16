using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire; // Add this using directive
using Microsoft.AspNetCore.Authorization;

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleCloudStorageService _gcsService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public DocumentsController(ApplicationDbContext context, GoogleCloudStorageService gcsService, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _gcsService = gcsService;
            _backgroundJobClient = backgroundJobClient;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
        {
            return await _context.Documents.Include(d => d.Course).ToListAsync();
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.DocumentChunks)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            return document;
        }

        // POST: api/Documents
        [HttpPost]
        public async Task<ActionResult<Document>> UploadDocument([FromForm] int courseId, [FromForm] string title, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            // Validate MimeType for Document AI
            var supportedMimeTypes = new List<string> { "application/pdf", "image/jpeg", "image/png", "image/gif" };
            if (!supportedMimeTypes.Contains(file.ContentType))
            {
                return BadRequest($"Unsupported file format. Supported formats: {string.Join(", ", supportedMimeTypes)}");
            }

            // Ensure the CourseId is valid and exists
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists)
            {
                return BadRequest("Invalid Course ID.");
            }

            // 1. Upload file to GCS
            var fileUrl = await _gcsService.UploadFileAsync(file, "documents");

            // 2. Create Document record
            var document = new Document
            {
                CourseId = courseId,
                Title = title,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                ProcessingStatus = "processing", // Set status to processing
                FileUrl = fileUrl
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            
            // 3. Enqueue the background job for processing
            byte[] fileContent;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileContent = memoryStream.ToArray();
            }

            _backgroundJobClient.Enqueue<IBackgroundJobService>(service => 
                service.ProcessDocumentAsync(document.Id, fileContent, file.ContentType));

            return AcceptedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }

        // PUT: api/Documents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, Document document)
        {
            if (id != document.Id)
            {
                return BadRequest();
            }

            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
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

        // DELETE: api/Documents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Optionally delete from GCS as well
            if (!string.IsNullOrEmpty(document.FileUrl))
            {
                try
                {
                    var objectName = new Uri(document.FileUrl).Segments.Last();
                    // Assuming objectName is just the last segment, might need more robust parsing
                    await _gcsService.DeleteFileAsync($"documents/{objectName}");
                }
                catch (Exception ex)
                {
                    // Log error but don't prevent database delete
                    Console.WriteLine($"Error deleting file from GCS: {ex.Message}");
                }
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}
