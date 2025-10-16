using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class UserPoint
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public int PointsEarned { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Reason { get; set; } // quiz_completion/daily_streak/etc

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
