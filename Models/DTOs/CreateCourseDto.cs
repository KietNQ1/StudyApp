using System.ComponentModel.DataAnnotations;

namespace myapp.Models.DTOs
{
    public class CreateCourseDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        public string? Description { get; set; }
    }
}
