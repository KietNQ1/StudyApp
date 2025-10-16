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

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleCloudStorageService _gcsService;
        private readonly DocumentProcessorService _docAIService;
        private readonly VertexAIService _vertexAIService;

        public DocumentsController(ApplicationDbContext context, GoogleCloudStorageService gcsService, DocumentProcessorService docAIService, VertexAIService vertexAIService)
        {
            _context = context;
            _gcsService = gcsService;
            _docAIService = docAIService;
            _vertexAIService = vertexAIService;
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

            var document = new Document
            {
                CourseId = courseId,
                Title = title,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                ProcessingStatus = "pending",
                FileUrl = string.Empty // Initialize FileUrl to avoid CS9035, will be updated after upload
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            try
            {
                // Upload to GCS
                var fileUrl = await _gcsService.UploadFileAsync(file, "documents"); // 'documents' is the folder in GCS
                document.FileUrl = fileUrl;

                // Read file content for Document AI processing
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                }

                // Extract text using Document AI, passing the correct MimeType
                var extractedText = await _docAIService.ExtractTextAsync(fileContent, file.ContentType);
                document.ExtractedText = extractedText;
                document.ProcessingStatus = "completed";
                document.ProcessedAt = DateTime.UtcNow;
                document.PageCount = 1; // Simplified, Document AI can provide page count

                // Process text into chunks and generate embeddings
                if (!string.IsNullOrEmpty(extractedText))
                {
                    const int chunkSize = 1000; // Example chunk size
                    var chunks = new List<string>();
                    for (int i = 0; i < extractedText.Length; i += chunkSize)
                    {
                        chunks.Add(extractedText.Substring(i, Math.Min(chunkSize, extractedText.Length - i)));
                    }

                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var chunkContent = chunks[i];
                        var embedding = await _vertexAIService.GenerateEmbeddingAsync(chunkContent);

                        _context.DocumentChunks.Add(new DocumentChunk
                        {
                            DocumentId = document.Id,
                            Content = chunkContent,
                            ChunkIndex = i,
                            PageNumber = 1, // Default to page 1 for simplicity and to avoid division by zero
                            EmbeddingVector = embedding,
                            TokenCount = chunkContent.Length / 4, // Estimate tokens
                            Metadata = "{}"
                        });
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                document.ProcessingStatus = "failed";
                // Log the exception (e.g., using Serilog)
                Console.WriteLine($"Error processing document: {ex.Message}");
                return StatusCode(500, $"Error uploading or processing document: {ex.Message}");
            }

            await _context.SaveChangesAsync(); // Save final document status and chunks

            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
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
