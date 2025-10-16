using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class DocumentChunk
    {
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        public Document? Document { get; set; } // Made nullable

        [Required]
        public required string Content { get; set; }
        public int ChunkIndex { get; set; }
        public int PageNumber { get; set; }

        [Column(TypeName = "vector(1536)")] // Configure for pgvector in PostgreSQL
        public float[]? EmbeddingVector { get; set; } // For semantic search

        public int TokenCount { get; set; }
        public string? Metadata { get; set; } // JSON
    }
}
