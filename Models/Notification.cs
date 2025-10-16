using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Type { get; set; } // quiz_due/streak_reminder/achievement/etc

        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
