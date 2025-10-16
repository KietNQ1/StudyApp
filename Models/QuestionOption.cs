using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; } // Made nullable

        [Required]
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }
}
