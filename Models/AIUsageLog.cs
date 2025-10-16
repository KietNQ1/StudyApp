using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class AIUsageLog
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FeatureType { get; set; } // chat/quiz_generation/grading

        public int TokensConsumed { get; set; }
        public decimal CostUsd { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
