using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class StudyReminder
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        [Required]
        public TimeSpan ReminderTime { get; set; } // TIME

        public string? DaysOfWeek { get; set; } // JSON array of days (e.g., ["Mon", "Wed", "Fri"])
        public bool IsActive { get; set; } = true;
    }
}
