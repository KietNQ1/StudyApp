using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public int SessionId { get; set; }

        [ForeignKey("SessionId")]
        public ChatSession? ChatSession { get; set; } // Made nullable to resolve CS9035

        [Required]
        [MaxLength(50)]
        public required string Role { get; set; } // user/assistant/system

        [Required]
        public required string Content { get; set; }

        public int TokensUsed { get; set; }
        public string? ModelVersion { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<MessageCitation> MessageCitations { get; set; } = new List<MessageCitation>();
    }
}
