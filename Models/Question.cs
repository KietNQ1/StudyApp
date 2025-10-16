using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Question
    {
        public int Id { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public int? TopicId { get; set; }
        [ForeignKey("TopicId")]
        public Topic? Topic { get; set; }

        public int? DocumentId { get; set; } // nếu sinh từ tài liệu
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        [Required]
        [MaxLength(50)]
        public required string QuestionType { get; set; } // multiple_choice/true_false/short_answer/essay

        [Required]
        public required string QuestionText { get; set; }

        [MaxLength(50)]
        public string? DifficultyLevel { get; set; } // easy/medium/hard

        public int Points { get; set; }
        public string? Explanation { get; set; }
        public bool GeneratedByAi { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
        public virtual ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
    }
}
