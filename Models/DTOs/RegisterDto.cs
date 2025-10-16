using System.ComponentModel.DataAnnotations;

namespace myapp.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? FullName { get; set; }
    }
}
