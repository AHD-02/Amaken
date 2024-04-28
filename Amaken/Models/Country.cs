using System.ComponentModel.DataAnnotations;

namespace Amaken.Models;

public class Country
{
    [Key]
    public int ID { get; set; }
    public string Name { get; set; }
    
    public string Code { get; set; }
}