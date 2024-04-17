using System.ComponentModel.DataAnnotations;

namespace Amaken.Models
{
    public enum AdminStatus
    {
        OK,
        Deleted,
        Suspended
    }
    public class Admin
    {
        
        [Key]
        public string? Email { get; set;}
        [Required]
        public string? Password { get; set;}
        [Required]
        public string? Name { get; set; }
        [Required]
        public string Status { get; set; } = "OK";


    }
}
