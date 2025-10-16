using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Achievement
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        public string? Description { get; set; }
        public string? BadgeIconUrl { get; set; }
        public string? Criteria { get; set; } // JSON
        public int PointsReward { get; set; }
    }
}
