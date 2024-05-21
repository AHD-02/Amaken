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
        private readonly NotificationController _NotificationController;
        public EventController(ApplicationDbContext context, NotificationController NotificationController)
        {
            _context = context;
            _NotificationController = NotificationController;
        }
        public int GetLastId()
        {
            using (var context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
            {
                return context.Event
                    .AsEnumerable() 
                    .Where(x => int.TryParse(x.EventId, out _)) 
                    .OrderByDescending(x => int.Parse(x.EventId!)) 
                    .Select(x => int.Parse(x.EventId!)) 
                    .FirstOrDefault(); 
            }
        }
        [HttpPost]
        [Route("api/[controller]/CreateEvent")]
        [Authorize]
        public IActionResult CreateEvent([FromBody] Event newEvent)
        {
            if (ModelState.IsValid)
            {
                
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var MyUser = _context.User.AsNoTracking().FirstOrDefault(u => u.Email!.Equals(userEmail));
                if (MyUser!= null)
                {
                    int lastId = GetLastId();
                    newEvent.EventId = $"{lastId + 1}";
                    newEvent.Status = "OK";
                    newEvent.UserEmail = MyUser.Email;
                    _context.Event.Add(newEvent);
                    _context.SaveChanges();
                    _NotificationController.PushNotifications($"The event {newEvent.Name} was added");
                    return Ok("Event is created");
                    
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
                var existingEvent = _context.Event.AsNoTracking().FirstOrDefault(u => u.EventId!.ToLower() == updatedEvent.EventId!.ToLower());
                if (existingEvent != null)
                {
                    existingEvent.Name = updatedEvent.Name;
                    existingEvent.PlaceID = updatedEvent.PlaceID;
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
                var Event = _context.Event.AsNoTracking().FirstOrDefault(u => u.EventId!.ToLower().Equals(EventId.ToLower()));
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
            var user = await _context.User.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);

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
            string TypeOfEventPlace = events.PlaceID.Split("-")[0];
            EventGetDto myNewEvent = new EventGetDto(events);
            if (TypeOfEventPlace.Equals("Private"))
            {
                var place =  _context.Private_Place.Where(u => u.PlaceId.Equals(events.PlaceID))
                    .FirstOrDefault();
            myNewEvent.Latitude = place.Latitude;
            myNewEvent.Longitude = place.Longitude;
            }
            else if (TypeOfEventPlace.Equals("Public"))
            {
            var place =  _context.Public_Place.AsNoTracking().Where(u => u.PublicPlaceId.Equals(events.PlaceID))
                .FirstOrDefault();
            myNewEvent.Latitude = place.Latitude;
            myNewEvent.Longitude = place.Longitude;
            myNewEvent.PlaceName = place.Name;
            }
            else
            {
                return BadRequest("Place ID isn't valid");
            }
            if (events == null)
                throw new Exception($"Event with id {id} was not found");
            
            return Ok(myNewEvent);
        }
        
        [HttpPost]
        [Route("api/[controller]/{eventId}/Save")]
        [Authorize]
        public IActionResult SaveEvent(string eventId)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.AsNoTracking().FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var myEvent = _context.Event.AsNoTracking().FirstOrDefault(u => u.EventId!.Equals(eventId));
                if (myEvent != null)
                {
                   
                    List<string> list = new List<string>(MyUser.SavedEvents ?? new string[0]);
                    if (!list.Contains(eventId))
                    {
                        list.Add(eventId);
                        MyUser.SavedEvents = list.ToArray();
                        _context.SaveChanges();
                        return Ok($"Event {eventId} is saved");
                    }

                    else
                    {
                        return BadRequest("Event is already saved");
                    } 
                }
                else
                {
                    return NotFound("Event wasn't found");
                }
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
        }
        
        [HttpPost]
        [Route("api/[controller]/{eventId}/UnSave")]
        [Authorize]
        public IActionResult UnSaveEvent(string eventId)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.AsNoTracking().FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var myEvent = _context.Event.AsNoTracking().FirstOrDefault(u => u.EventId!.Equals(eventId));
                if (myEvent != null)
                {
                    List<string> list = new List<string>(MyUser.SavedEvents ?? new string[0]);
                    if (list.Contains(eventId))
                    {
                        list.Remove(eventId);
                        MyUser.SavedEvents = list.ToArray();
                        _context.SaveChanges();
                        return Ok($"Event {eventId} is removed");
                    }

                    else
                    {
                        return BadRequest("Event isn't saved");
                    } 
                }
                else
                {
                    return NotFound("Event wasn't found");
                }
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
        }
        
        [HttpGet]
        [Route("api/[controller]/IsNameUnique")]
        public ActionResult<bool> IsNameUnique(string name)
        {
            bool isUnique = !_context.Event.AsNoTracking().Any(p => p.Name.ToLower() == name.ToLower());
            return Ok(isUnique);
        }
    }
}
