using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using myapp.Models;

namespace myapp.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // người tạo

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Subject { get; set; }

        public string? ThumbnailUrl { get; set; }
        public bool IsPublic { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>(); // Add this line
    }
}
