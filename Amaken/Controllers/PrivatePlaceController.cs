using System.Security.Claims;
using Amaken.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amaken.Controllers
{
    public class Private_PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationController _NotificationController;
        public Private_PlaceController(ApplicationDbContext context, NotificationController NotificationController)
        {
            _context = context;
            _NotificationController = NotificationController;
        }
        public int GetLastId()
        {
            using (var context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
            {
                return context.Private_Place
                    .AsEnumerable() 
                    .Where(x => int.TryParse(x.PlaceId.Split("-")[1], out _)) 
                    .OrderByDescending(x => int.Parse(x.PlaceId!.Split("-")[1])) 
                    .Select(x => int.Parse(x.PlaceId!.Split("-")[1])) 
                    .FirstOrDefault(); 
            }
        }
        [HttpPost]
        [Route("api/[controller]/CreatePriavtePlace")]
        [Authorize]
        public IActionResult CreatePriavtePlace([FromBody] Private_Place myPrivate_Place)
        {
            if (ModelState.IsValid)
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
                if (MyUser != null)
                {
                    
                myPrivate_Place.AddedOn = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                myPrivate_Place.Status = "Unapproved";
                myPrivate_Place.UserEmail = MyUser.Email;
                int lastId = GetLastId();
                myPrivate_Place.PlaceId = $"Private-{lastId + 1}";
                _context.Private_Place.Add(myPrivate_Place);
                _context.SaveChanges();
                _NotificationController.PushNotifications($"The private place {myPrivate_Place.PlaceName} was added");
                return Ok("Private place was added successfully, needs admin's approval");
                }
                else
                {
                    return Unauthorized("User isn't authorized");
                }
            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/TriggerPriavtePlaceStatus")]
        public IActionResult TriggerPriavtePlaceStatus(string id, string status)
        {
            if (ModelState.IsValid)
            {
                var PrivatePlace = _context.Private_Place.FirstOrDefault(u => u.PlaceId!.Equals(id));
                if (PrivatePlace != null)
                {
                    if (Enum.IsDefined(typeof(Private_Place_Status), status))
                    {
                        PrivatePlace.Status = status;
                        _context.SaveChanges();
                        return Ok("Status was successfully triggered to " + status);
                    }
                    else
                    {
                        return BadRequest("Status isn't defined");
                    }
                }
                else
                {
                    return NotFound("Private place wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/UpdatePriavtePlace")]
        public IActionResult UpdatePriavtePlace([FromBody] Private_Place updatedPlace)
        {
            if (ModelState.IsValid)
            {
                var PrivatePlace = _context.Private_Place.FirstOrDefault(u => u.PlaceId!.Equals(updatedPlace.PlaceId));
                if (PrivatePlace != null)
                {
                    updatedPlace.AddedOn = DateTime.SpecifyKind(updatedPlace.AddedOn, DateTimeKind.Utc);
                    updatedPlace.AvailableFrom = DateTime.SpecifyKind(updatedPlace.AvailableFrom, DateTimeKind.Utc);
                    updatedPlace.AvailableTo = DateTime.SpecifyKind(updatedPlace.AvailableTo, DateTimeKind.Utc);
                    PrivatePlace.UserEmail = updatedPlace.UserEmail;
                    PrivatePlace.Longitude = updatedPlace.Longitude;
                    PrivatePlace.Latitude = updatedPlace.Latitude;
                    PrivatePlace.RegisterNumber= updatedPlace.RegisterNumber;
                    PrivatePlace.Description=updatedPlace.Description;
                    PrivatePlace.Status=updatedPlace.Status;
                    PrivatePlace.PlaceName=updatedPlace.PlaceName;
                    PrivatePlace.Images=updatedPlace.Images;
                    PrivatePlace.ImageOfOwnershipProof = updatedPlace.ImageOfOwnershipProof;
                    PrivatePlace.ImageOfOwnerID = updatedPlace.ImageOfOwnerID;
                    PrivatePlace.CategoryID = updatedPlace.CategoryID;
                    _context.SaveChanges();
                    return Ok("Private place was updated successfully");
                }
                else
                {
                    return NotFound("Private place wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpGet]
        [Route("api/[controller]/SearchPrivatePlaces")]
        public IActionResult SearchPrivatePlaces()
        {
            var PrivatePlaces = _context.Private_Place.Select(place => new
                {
                    Place = place,
                    NumberOfRates = _context.PlacesRates.Count(r => r.PlaceId.ToLower().Equals(place.PlaceId.ToLower())),
                    AverageScore = _context.PlacesRates.Where(r => r.PlaceId.ToLower().Equals(place.PlaceId.ToLower())).Average(r => (double?)r.Score) ?? 0
                })
                .OrderByDescending(p => p.Place.AddedOn)
                .ToList();
            return Ok(PrivatePlaces);
        }
        [HttpGet]
        [Route("api/[controller]/GetScore")]
        public IActionResult GetScore(string id)
        {
            return Ok(_context.PlacesRates.Where(u => u.PlaceId.Equals(id)).Average(u => u.Score));
        }
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetPlaces(string id)
        {
            var placeWithRates = await _context.Private_Place
                .Where(e => e.PlaceId.ToLower() == id.ToLower())
                .Select(place => new
                {
                    Place = place,
                    NumberOfRates = _context.PlacesRates.Count(r => r.PlaceId.ToLower() == place.PlaceId.ToLower()),
                    AverageScore = _context.PlacesRates
                        .Where(r => r.PlaceId.ToLower() == place.PlaceId.ToLower())
                        .Select(r => (double?)r.Score)
                        .Average() ?? 0
                })
                .FirstOrDefaultAsync();

            if (placeWithRates == null)
            {
                return NotFound();
            }

            return Ok(placeWithRates);
        }

    }
}
