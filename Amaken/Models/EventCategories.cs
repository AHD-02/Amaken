using System.ComponentModel.DataAnnotations;

namespace Amaken.Models;

public class EventCategories
{
    [Key]
    public string ID { get; set; }
    
    [Required]
    public string Name { get; set; }
}