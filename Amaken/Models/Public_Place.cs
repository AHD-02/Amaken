using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{
    public enum Public_Place_Status
    {
        OK,
        Deleted,
        Suspended
    }
    public class Public_Place
    {
        [Key]
        public string? PublicPlaceId { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public string[] Images { get; set; } 
        [Required]
        public string? Location { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? UserEmail { get; set; }
        public string? Status { get; set; }
        public DateTime? AddedOn { get; set; }
        
        [Required]
        public double Longitude { get; set; }
        
        [Required]
        public double Latitude { get; set; }
        [Required]
        public string CategoryID { get; set; }

    }
}
