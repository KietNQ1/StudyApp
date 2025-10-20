using System.ComponentModel.DataAnnotations;

namespace myapp.Models.DTOs
{
    public class GoogleLoginRequestDto
    {
        [Required]
        public required string Credential { get; set; }
    }
}
