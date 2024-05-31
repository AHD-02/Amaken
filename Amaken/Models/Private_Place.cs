using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{
    public enum Private_Place_Status
    {
        OK,
        Rejected,
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
        [Required]
        public string[] Images { get; set; } 
        public string? Description { get; set; }
        public string? UserEmail { get; set; } 
        [Required]
        public String Status { get; set; } = "Unapproved";
        public DateTime AddedOn { get; set; }
        [Required]
        public string ImageOfOwnerID { get; set; }
        [Required]
        public string ImageOfOwnershipProof{ get; set; }
        
        [Required]
        public string CategoryID{ get; set; }
        
        [Required]
        public double Longitude { get; set; }
        
        [Required]
        public double Latitude { get; set; }

        public DateTime? AvailableFrom { get; set; } 

        public DateTime? AvailableTo { get; set; } 



    }
}
