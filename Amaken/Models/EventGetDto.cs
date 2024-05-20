using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{
    public class EventGetDto
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
        public DateTimeOffset  EventStart { get; set; }
        [Required]
        public DateTimeOffset  EventEnd { get; set; }
        [Required]
        public double Fees { get; set; }
        public string? UserEmail { get; set; }
        public string Status { get; set; } = "OK";
        [Required]
        public string[] Images { get; set; } 
        
        [Required]
        public double Longitude { get; set; }
        
        [Required]
        public double Latitude { get; set; }
        
        public EventGetDto (Event myEvent)
        {
            this.Description = myEvent.Description;
            this.EventId = myEvent.EventId;
            this.EventType = myEvent.EventType;
            this.Status = myEvent.Status;
            this.Location = myEvent.Location;
            this.Images = myEvent.Images;
            this.Fees = myEvent.Fees;
            this.EventEnd = myEvent.EventEnd;
            this.UserEmail = myEvent.UserEmail;
            this.EventStart = myEvent.EventStart;
            this.Name = myEvent.Name;
        }
    }
}