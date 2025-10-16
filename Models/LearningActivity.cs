using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class LearningActivity
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public int? DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        [Required]
        [MaxLength(50)]
        public required string ActivityType { get; set; } // read/chat/quiz/review

        public int DurationMinutes { get; set; }
        public string? Metadata { get; set; } // JSON chi tiết cụ thể
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
