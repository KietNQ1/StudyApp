using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; } // Made nullable to resolve CS9035

        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        [MaxLength(50)]
        public string? FileType { get; set; } // pdf/docx/pptx/video/audio

        [Required]
        public required string FileUrl { get; set; }

        public long FileSize { get; set; }

        [MaxLength(50)]
        public string ProcessingStatus { get; set; } = "pending"; // pending/processing/completed/failed

        public string? ExtractedText { get; set; } // cho full-text search
        public int PageCount { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        public virtual ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();
    }
}
