using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amaken.Models
{

    public class Public_Place
    {
        [Key]
        public string? PublicPlaceId { get; set; }
        [Required]
        public string? Description { get; set; }
        public List<ImageUri> Images { get; set; } = new List<ImageUri>();
        [Required]
        public string? Location { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? UserEmail { get; set; } 


    }
}
