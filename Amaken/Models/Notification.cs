using System.ComponentModel.DataAnnotations;

namespace Amaken.Models;

public class Notification
{
    [Key]
    public string? ID { get; set; }
    public string? UserEmail { get; set; }
    public DateTime? ReadOn { get; set; }
    public DateTime? CreatedOn { get; set; }
    [Required]
    public string Description { get; set; }

    public Notification(string? userEmail, string description)
    {
        this.Description = description;
        this.UserEmail = userEmail;
    }
}