using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class UserAchievement
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        [Required]
        public int AchievementId { get; set; }
        [ForeignKey("AchievementId")]
        public required Achievement Achievement { get; set; }

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    }
}
