using System.ComponentModel.DataAnnotations;

namespace Amaken.Models;

public class PrivatePlacesCategories
{
    [Key]
    public string ID { get; set; }
    
    [Required]
    public string Name { get; set; }
}