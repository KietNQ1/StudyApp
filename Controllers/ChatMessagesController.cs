using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pgvector.EntityFrameworkCore; // For vector search with PostgreSQL

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly VertexAIService _vertexAIService;

        public ChatMessagesController(ApplicationDbContext context, VertexAIService vertexAIService)
        {
            _context = context;
            _vertexAIService = vertexAIService;
        }

        // GET: api/ChatMessages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetChatMessages()
        {
            return await _context.ChatMessages.Include(cm => cm.ChatSession).ToListAsync();
        }

        // GET: api/ChatMessages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatMessage>> GetChatMessage(int id)
        {
            var chatMessage = await _context.ChatMessages
                .Include(cm => cm.ChatSession)
                .Include(cm => cm.MessageCitations)
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (chatMessage == null)
            {
                return NotFound();
            }

            return chatMessage;
        }

        // POST: api/ChatMessages
        [HttpPost]
        public async Task<ActionResult<object>> PostChatMessage(ChatMessage chatMessage)
        {
            var chatSession = await _context.ChatSessions
                .Include(cs => cs.Document)
                .FirstOrDefaultAsync(cs => cs.Id == chatMessage.SessionId);

            if (chatSession == null)
            {
                return BadRequest("Invalid Chat Session ID.");
            }

            // 1. Save user message
            chatMessage.Role = "user";
            chatMessage.CreatedAt = DateTime.UtcNow;
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            string aiResponseContent;
            List<MessageCitation> citations = new List<MessageCitation>();

            try
            {
                string documentContext = "";
                if (chatSession.Document != null && !string.IsNullOrEmpty(chatSession.Document.ExtractedText))
                {
                    // Generate embedding for user query
                    var userQueryEmbedding = await _vertexAIService.GenerateEmbeddingAsync(chatMessage.Content);

                    // Perform vector search for relevant document chunks
                    var relevantChunks = await _context.DocumentChunks
                        .Where(dc => dc.DocumentId == chatSession.Document.Id && dc.EmbeddingVector != null)
                        .OrderByDescending(dc => dc.EmbeddingVector.CosineDistance(userQueryEmbedding)) // For PostgreSQL with pgvector
                        .Take(5) // Get top 5 most relevant chunks
                        .ToListAsync();

                    if (relevantChunks.Any())
                    {
                        documentContext = string.Join("\n\n", relevantChunks.Select(dc => dc.Content));

                        // Create citations
                        foreach (var chunk in relevantChunks)
                        {
                            citations.Add(new MessageCitation
                            {
                                DocumentId = chunk.DocumentId,
                                ChunkId = chunk.Id,
                                PageNumber = chunk.PageNumber,
                                QuoteText = chunk.Content.Length > 200 ? chunk.Content.Substring(0, 200) + "..." : chunk.Content,
                                RelevanceScore = 1.0, // Placeholder, actual score can be calculated
                                ChatMessage = chatMessage // Link to the AI message, will be updated later
                            });
                        }
                    }
                    else
                    {
                        // Fallback to full extracted text if no relevant chunks found
                        documentContext = chatSession.Document.ExtractedText.Length > 4000 
                                        ? chatSession.Document.ExtractedText.Substring(0, 4000) 
                                        : chatSession.Document.ExtractedText;
                    }
                }
                
                // Get AI response
                if (!string.IsNullOrEmpty(documentContext))
                {
                    aiResponseContent = await _vertexAIService.ChatWithDocument(chatMessage.Content, documentContext);
                }
                else
                {
                    aiResponseContent = await _vertexAIService.PredictTextAsync(chatMessage.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating AI response or vector search: {ex.Message}");
                aiResponseContent = "Sorry, I am having trouble processing your request right now.";
            }

            // 4. Save AI message
            var aiMessage = new ChatMessage
            {
                SessionId = chatMessage.SessionId,
                Role = "assistant",
                Content = aiResponseContent,
                CreatedAt = DateTime.UtcNow,
                // TokensUsed and ModelVersion can be populated from AI service response if available
            };
            _context.ChatMessages.Add(aiMessage);
            await _context.SaveChangesAsync();

            // Link citations to the AI message and save them
            foreach(var citation in citations)
            {
                citation.MessageId = aiMessage.Id;
                _context.MessageCitations.Add(citation);
            }
            await _context.SaveChangesAsync();
            
            // Update last message time for the session
            chatSession.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { userMessage = chatMessage, aiResponse = aiMessage, citations = citations });
        }

        // PUT: api/ChatMessages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChatMessage(int id, ChatMessage chatMessage)
        {
            if (id != chatMessage.Id)
            {
                return BadRequest();
            }

            _context.Entry(chatMessage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatMessageExists(id))
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

        // DELETE: api/ChatMessages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatMessage(int id)
        {
            var chatMessage = await _context.ChatMessages.FindAsync(id);
            if (chatMessage == null)
            {
                return NotFound();
            }

            _context.ChatMessages.Remove(chatMessage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChatMessageExists(int id)
        {
            return _context.ChatMessages.Any(e => e.Id == id);
        }
    }
}
