using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class MessageCitation
    {
        public int Id { get; set; }

        [Required]
        public int MessageId { get; set; }

        [ForeignKey("MessageId")]
        public required ChatMessage ChatMessage { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        public Document? Document { get; set; } // Made nullable

        public int? ChunkId { get; set; }
        [ForeignKey("ChunkId")]
        public DocumentChunk? DocumentChunk { get; set; }

        public int PageNumber { get; set; }
        public string? QuoteText { get; set; }
        public double RelevanceScore { get; set; }
    }
}
