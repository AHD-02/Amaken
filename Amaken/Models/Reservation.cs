using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{
    public class Reservation
    {
        [Key]
        public string? ReservationId { get; set; }
        public string? EventId { get; set; } 
        public string? UserEmail { get; set; }
        public DateTime DateOfReservation { get; set; }
    }
}
