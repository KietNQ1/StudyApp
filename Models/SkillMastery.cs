using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class SkillMastery
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

        [Required]
        public int TopicId { get; set; }
        [ForeignKey("TopicId")]
        public required Topic Topic { get; set; }

        public int MasteryLevel { get; set; } // 0-100
        public int QuestionsAttempted { get; set; }
        public int QuestionsCorrect { get; set; }
        public DateTime? LastPracticedAt { get; set; }
        public bool NeedsReview { get; set; }
    }
}
