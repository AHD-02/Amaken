using System.ComponentModel.DataAnnotations;

namespace Amaken.Models;

public class PublicPlacesCategories
{
    [Key]
    public string ID { get; set; }
    
    [Required]
    public string Name { get; set; }
    
}