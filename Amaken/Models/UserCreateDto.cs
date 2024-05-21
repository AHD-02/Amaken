using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Amaken.Models
{
    
    public class UserCreateDto
    {

        [Key]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string? Phone { get; set; }
        public string[]? Images { get; set; } 
        public string[]? SavedEvents { get; set; } 
        [Required]
        public string? Country { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string[]? Intrests { get; set; }

        [Required]
        public string? Status { get; set; } = "OK";
        
        


    }
}