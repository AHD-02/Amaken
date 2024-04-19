using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{
    public enum EventStatus
    {
        OK,
        Cancelled,
        Deleted
    }
    public class Event
    {
        [Key]
        public string? EventId { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Location { get; set; }
        [Required]
        public string? EventType { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public DateTime EventStart { get; set; }
        [Required]
        public DateTime EventEnd { get; set; }
        [Required]
        public double Fees { get; set; }
        [Required]
        public string? UserEmail { get; set; }
        [Required]
        public string Status { get; set; } = "OK";
        [Required]
        public string[] Images { get; set; } 

    }
}
