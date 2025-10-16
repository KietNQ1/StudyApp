using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; } // Made nullable

        public string? GradeLevel { get; set; }
        public string? Major { get; set; }
        public string? LearningStyle { get; set; } // visual/auditory/kinesthetic
        public string? Goals { get; set; }
        public string? Preferences { get; set; } // JSON format
    }
}
