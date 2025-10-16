using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class CourseProgress
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public required Course Course { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastAccessedAt { get; set; }
        public double CompletionPercentage { get; set; }
        public int TimeSpentMinutes { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Status { get; set; } // not_started/in_progress/completed
    }
}
