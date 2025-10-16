using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Topic
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public required Course Course { get; set; }

        public int? ParentTopicId { get; set; }

        [ForeignKey("ParentTopicId")]
        public virtual Topic? ParentTopic { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public int EstimatedTimeMinutes { get; set; }

        public virtual ICollection<Topic> ChildTopics { get; set; } = new List<Topic>();
    }
}
