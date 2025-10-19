using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class QuizAnswer
    {
        public int Id { get; set; }

        [Required]
        public int AttemptId { get; set; }

        [ForeignKey("AttemptId")]
        public QuizAttempt? QuizAttempt { get; set; } // Made nullable

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; } // Made nullable

        public int? SelectedOptionId { get; set; }
        [ForeignKey("SelectedOptionId")]
        public QuestionOption? SelectedOption { get; set; }

        public string? TextAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public double PointsEarned { get; set; }
        public string? AiFeedback { get; set; } // cho câu hỏi tự luận
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
