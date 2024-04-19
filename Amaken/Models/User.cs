﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Required]
        public string? Country { get; set; }
        [Required]
        public string? City { get; set; }

        [Required]
        public string? Status { get; set; } = "OK";


    }
}
