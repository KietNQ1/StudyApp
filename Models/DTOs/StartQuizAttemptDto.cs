using System.ComponentModel.DataAnnotations;

namespace myapp.Models.DTOs
{
    public class StartQuizAttemptDto
    {
        [Required]
        public int QuizId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
