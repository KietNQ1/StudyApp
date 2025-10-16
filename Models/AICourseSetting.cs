using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class AICourseSetting
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public required Course Course { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ModelName { get; set; }

        public double Temperature { get; set; } = 0.7; // Default temperature
        public int MaxTokens { get; set; } = 1024; // Default max tokens
        public string? SystemPrompt { get; set; }
        public bool RagEnabled { get; set; } = true;
        public int RagTopK { get; set; } = 5;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
