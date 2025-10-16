using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Role { get; set; } // student/teacher/admin
        public string? SubscriptionTier { get; set; } // free/premium/enterprise
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public StudentProfile? StudentProfile { get; set; }
    }
}
