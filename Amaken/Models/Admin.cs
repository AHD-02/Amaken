using System.ComponentModel.DataAnnotations;

namespace Amaken.Models
{
    public class Admin
    {
        
        [Key]
        public string? Email { get; set;}
        [Required]
        public string? Password { get; set;}
        [Required]
        public string? Name { get; set; }

        
    }
}
