using System.Security.Claims;
using Amaken.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amaken.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EventController(ApplicationDbContext context)
        {
            _context = context; 
        }
        [HttpPost]
        [Route("api/[controller]/CreateEvent")]
        [Authorize]
        public IActionResult CreateEvent([FromBody] Event newEvent)
        {
            if (ModelState.IsValid)
            {
                
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
                if (MyUser!= null)
                {
                    var user = _context.User.FirstOrDefault(u => u.Email!.ToLower() == newEvent.UserEmail!.ToLower());
                    if (user != null)
                    {
                        newEvent.Status = "OK";
                        _context.Event.Add(newEvent);
                        _context.SaveChanges();
                        return Ok("Event is created");
                    }
                    else
                    {
                        return NotFound("User wasn't found");
                    }
                }
                else
                {
                    return Unauthorized("User isn't authorized");
                }


            }
            else return BadRequest("Data is invalid");
        }
        [HttpPut]
        [Route("api/[controller]/UpdateEvent")]
        public IActionResult UpdateEvent([FromBody] Event updatedEvent)
        {
            if (ModelState.IsValid)
            {
                var existingEvent = _context.Event.FirstOrDefault(u => u.EventId!.ToLower() == updatedEvent.EventId!.ToLower());
                if (existingEvent != null)
                {
                    existingEvent.Name = updatedEvent.Name;
                    existingEvent.Location = updatedEvent.Location;
                    existingEvent.EventType = updatedEvent.EventType;
                    existingEvent.Description = updatedEvent.Description;
                    existingEvent.EventStart = updatedEvent.EventStart;
                    existingEvent.EventEnd = updatedEvent.EventEnd;
                    existingEvent.Fees = updatedEvent.Fees;
                    existingEvent.UserEmail = updatedEvent.UserEmail;
                    existingEvent.Status = updatedEvent.Status;
                    existingEvent.Images = updatedEvent.Images;
                    _context.SaveChanges();
                    return Ok(existingEvent.EventId + " is updated");

                }
                else
                {
                    return NotFound("Event was not found, please check the event id");
                }


            }
            else return BadRequest("Data is invalid");
        }
        [HttpPut]
        [Route("api/[controller]/TriggerEventStatus")]
        public IActionResult TriggerEventStatus(string EventId, string newStatus)
        {
            if (ModelState.IsValid)
            {
                var Event = _context.Event.FirstOrDefault(u => u.EventId!.ToLower().Equals(EventId.ToLower()));
                if (Event != null)
                {
                    if (Enum.IsDefined(typeof(EventStatus), newStatus))
                    {
                        Event.Status = newStatus;
                        _context.SaveChanges();
                        return Ok("Event status triggered to " + newStatus);
                    }
                    else
                    {
                        return BadRequest("Status is undefined");
                    }
                }
                else
                {
                    return NotFound("Event wasn't found");

                }

            }
            else return BadRequest("Data is invalid");
        }
        [HttpGet]
        [Route("api/[controller]/SearchEvents")]
        public IActionResult SearchEvents()
        {
            var Events = _context.Event.ToList();

            return Ok(Events);
        }
        
        [HttpGet]
        [Route("api/[controller]/SearchSavedEvents")]
        [Authorize]
        public async Task<IActionResult> SearchSavedEvents()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                throw new UnauthorizedAccessException("User was not found");

            if (user.SavedEvents == null)
                return Ok(new List<Event>());
            
            var events = await _context.Event
                .AsNoTracking()
                .Where(e => user.SavedEvents.Contains(e.EventId))
                .ToListAsync();

            return Ok(events);
        }
        
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult>GetEvent(string id)
        {
            var events = await _context.Event
                .AsNoTracking()
                .Where(e => e.EventId == id)
                .FirstOrDefaultAsync();
            if (events == null)
                throw new Exception($"Event with id {id} was not found");
            
            return Ok(events);
        }
        [HttpGet]
        [Route("api/[controller]/IsNameUnique")]
        public ActionResult<bool> IsNameUnique(string name)
        {
            bool isUnique = !_context.Event.Any(p => p.Name.ToLower() == name.ToLower());
            return Ok(isUnique);
        }
    }
}
