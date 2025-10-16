using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; } // Made nullable

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; } // Made nullable

        public int OrderIndex { get; set; }
        public int PointsOverride { get; set; } // Cho phép ghi đè điểm của câu hỏi trong quiz cụ thể
    }
}
