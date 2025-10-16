using myapp.Data;
using myapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly ApplicationDbContext _context;
        private readonly DocumentProcessorService _docAIService;
        private readonly VertexAIService _vertexAIService;

        public BackgroundJobService(ApplicationDbContext context, DocumentProcessorService docAIService, VertexAIService vertexAIService)
        {
            _context = context;
            _docAIService = docAIService;
            _vertexAIService = vertexAIService;
        }

        public async Task ProcessDocumentAsync(int documentId, byte[] fileContent, string mimeType)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                // Log or handle the case where the document is not found
                Console.WriteLine($"Document with ID {documentId} not found.");
                return;
            }

            try
            {
                // Extract text using Document AI
                var extractedText = await _docAIService.ExtractTextAsync(fileContent, mimeType);
                document.ExtractedText = extractedText;
                document.ProcessingStatus = "completed";
                document.ProcessedAt = DateTime.UtcNow;

                // Process text into chunks and generate embeddings
                if (!string.IsNullOrEmpty(extractedText))
                {
                    const int chunkSize = 1000;
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
                            PageNumber = 1, // Simplified
                            EmbeddingVector = embedding,
                            TokenCount = chunkContent.Length / 4, // Estimate
                            Metadata = "{}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                document.ProcessingStatus = "failed";
                // Log the exception
                Console.WriteLine($"Error processing document {documentId}: {ex.Message}");
            }
            finally
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
