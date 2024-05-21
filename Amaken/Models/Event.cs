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
        public string? PlaceID { get; set; }
        [Required]
        public string? EventType { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public DateTimeOffset  EventStart { get; set; }
        [Required]
        public DateTimeOffset  EventEnd { get; set; }
        [Required]
        public double Fees { get; set; }
        public string? UserEmail { get; set; }
        public string Status { get; set; } = "OK";
        [Required]
        public string[] Images { get; set; } 

    }
}
