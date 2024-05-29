using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Amaken.Models
{
    public enum UserStatus
    {
        OK,
        Deleted,
        Suspended
    }
    public class User
    {

        [Key]
        public string? Email { get; set; }
        [Required]
        [JsonIgnore]
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
        public string[]? SavedPublicPlaces { get; set; }
        [Required]
        public string? Country { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string[]? Intrests { get; set; }
        
        [Required]
        public string? Status { get; set; } = "OK";

        public User(UserCreateDto createUserDto)
        {
            this.City = createUserDto.City;
            this.Status = createUserDto.Status;
            this.Email = createUserDto.Email;
            this.FirstName = createUserDto.FirstName;
            this.Country = createUserDto.Country;
            this.Images = createUserDto.Images;
            this.Password = createUserDto.Password;
            this.Phone = createUserDto.Phone;
            this.LastName = createUserDto.LastName;
            this.SavedEvents = createUserDto.SavedEvents;
            this.DateOfBirth = createUserDto.DateOfBirth;
            this.Intrests = createUserDto.Intrests;
            this.SavedPublicPlaces = createUserDto.SavedPublicPlaces;
        }

        public User()
        {
            
        }


    }
}
