using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{
    public enum ReservationStatus
    {
        OK,
        Deleted,
        Cancelled
    }
    public class Reservation
    {
        [Key]
        public string? ReservationId { get; set; }
        public string? EventId { get; set; }
        public string? UserEmail { get; set; }
        public DateTime DateOfReservation { get; set; }
        [Required]
        public string? Status { get; set; } = "OK";
    }
}
