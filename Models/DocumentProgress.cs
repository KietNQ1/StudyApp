using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class DocumentProgress
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        [Required]
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public required Document Document { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public double CompletionPercentage { get; set; }
        public int TimeSpentMinutes { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}
