using System.ComponentModel.DataAnnotations;

namespace Amaken.Models
{
    public class ImageUri
    {
        [Key]
        public int Id { get; set; }
        public string? Uri { get; set; }
    }
}
