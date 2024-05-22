using System.Security.Claims;
using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amaken.Controllers;

public class NotificationController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }
    public int GetLastId()
    {
        using (var context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
        {
            return context.Notification
                .AsEnumerable() 
                .Where(x => int.TryParse(x.ID, out _)) 
                .OrderByDescending(x => int.Parse(x.ID!)) 
                .Select(x => int.Parse(x.ID!)) 
                .FirstOrDefault(); 
        }
    }
    [HttpPost]
    [Route("api/[controller]/Create")]
    public IActionResult Create([FromBody] Notification notification)
    {
        if (ModelState.IsValid)
        {
            notification.CreatedOn = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            int lastId = GetLastId();
            notification.ID = $"{lastId + 1}";
            _context.Notification.Add(notification);
            _context.SaveChanges();
            return Ok("Noification is created successfully");
            
        }
        else
        {
            return BadRequest("Invalid date");
        }
    }

    public void PushNotifications(string desc)
    {
        var users = _context.User.ToList();
        foreach (var user in users)
        {
            Notification notification = new Notification(user.Email,desc);
            Create(notification);
        }
    }
}