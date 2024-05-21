using System.ComponentModel.DataAnnotations;

namespace Amaken.Models;

public class PlacesRates
{
   [Key]
public string? UserEmail { get; set; }  
   [Key]
public string? PlaceId { get; set; }
[Required]
public int Score { get; set; }
}