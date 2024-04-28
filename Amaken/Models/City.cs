using System.ComponentModel.DataAnnotations;

namespace Amaken.Models;

public class City
{
    [Key]
    public int ID { get; set; }
    
    public string Name { get; set; }
    
    public string Country_Code { get; set; }
    
}