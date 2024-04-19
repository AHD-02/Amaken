using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{
    public enum Private_Place_Status
    {
        OK,
        Deleted,
        Suspended,
        Unapproved
    }
    public class Private_Place
    {
        [Key]
        public string? PlaceId { get; set; }
        public string? RegisterNumber { get; set; }
        public string? PlaceName { get; set; }
        public string? Location { get; set; }
        [Required]
        public string[] Images { get; set; } 
        public string? Description { get; set; }
        [Required]
        public string? UserEmail { get; set; } 
        [Required]
        public String Status { get; set; } = "Unapproved";
        public DateTime AddedOn { get; set; }


    }
}
