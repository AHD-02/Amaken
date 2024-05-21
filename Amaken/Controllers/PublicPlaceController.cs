using System.Security.Claims;
using Amaken.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Amaken.Controllers;

namespace Amaken.Controllers
{
    public class Public_PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationController _NotificationController;
        public Public_PlaceController(ApplicationDbContext context, NotificationController NotificationController)
        {
            _context = context;
            _NotificationController = NotificationController;
        }
        
        public int GetLastId()
        {
            using (var context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
            {
                return context.Public_Place
                    .AsEnumerable() 
                    .Where(x => int.TryParse(x.PublicPlaceId.Split("-")[1], out _)) 
                    .OrderByDescending(x => int.Parse(x.PublicPlaceId.Split("-")[1]!)) 
                    .Select(x => int.Parse(x.PublicPlaceId.Split("-")[1]!)) 
                    .FirstOrDefault(); 
            }
        }
        [HttpPost]
        [Route("api/[controller]/Create")]
        [Authorize]
        public IActionResult Create([FromBody] Public_Place myPublic_Place)
        {
            if (ModelState.IsValid)
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
                if (MyUser != null)
                { 
                int lastId = GetLastId();
                myPublic_Place.PublicPlaceId = $"Public-{lastId + 1}";
                myPublic_Place.AddedOn = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                myPublic_Place.Status = "OK";
                myPublic_Place.UserEmail = MyUser.Email;
                _context.Public_Place.Add(myPublic_Place);
                _context.SaveChanges();
                _NotificationController.PushNotifications($"The public place {myPublic_Place.Name} was added");
                return Ok("Public place was added successfully");
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
        [Route("api/[controller]/TriggerStatus")]
        public IActionResult TriggerPublicPlaceStatus(string id, string status)
        {
            if (ModelState.IsValid)
            {
                var PublicPlace = _context.Public_Place.AsNoTracking().FirstOrDefault(u => u.PublicPlaceId!.Equals(id));
                if (PublicPlace != null)
                {
                    if (Enum.IsDefined(typeof(Public_Place_Status), status))
                    {
                        PublicPlace.Status = status;
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
                    return NotFound("Public place wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/Update")]
        public IActionResult UpdatePublicPlace([FromBody] Public_Place updatedPlace)
        {
            if (ModelState.IsValid)
            {
                var PublicPlace = _context.Public_Place.FirstOrDefault(u => u.PublicPlaceId!.Equals(updatedPlace.PublicPlaceId));
                if (PublicPlace != null)
                {
                    PublicPlace.UserEmail = updatedPlace.UserEmail;
                    PublicPlace.Longitude = updatedPlace.Longitude;
                    PublicPlace.Latitude = updatedPlace.Latitude;
                    PublicPlace.Description = updatedPlace.Description;
                    PublicPlace.Status = updatedPlace.Status;
                    PublicPlace.Name = updatedPlace.Name;
                    PublicPlace.Images = updatedPlace.Images;
                    PublicPlace.CategoryID = updatedPlace.CategoryID;
                    _context.SaveChanges();
                    return Ok("Public place was updated successfully");
                }
                else
                {
                    return NotFound("Public place wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpGet]
        [Route("api/[controller]/Search")]
        public IActionResult SearchPublicPlaces()
        {
            var PublicPlaces = _context.Public_Place.ToList();

            return Ok(PublicPlaces);
        }
        
        
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult>GetPlace(string id)
        {
            var place = await _context.Public_Place
                .AsNoTracking()
                .Where(e => e.PublicPlaceId == id)
                .FirstOrDefaultAsync();
            if (place == null)
                throw new Exception($"Event with id {id} was not found");
            
            return Ok(place);
        }
        
        [HttpGet]
        [Route("api/[controller]/Get")]
        public IActionResult GetPlaces()
        {
            var Places = _context.Public_Place.AsNoTracking().Select(Place => new Public_Place
            {
                CategoryID = Place.CategoryID,
                Description = Place.Description,
                Images = Place.Images,
                Latitude = Place.Latitude,
                Longitude = Place.Longitude,
                Name = Place.Name,
                Status = Place.Status,
                AddedOn = Place.AddedOn,
                UserEmail = Place.UserEmail,
                PublicPlaceId = Place.PublicPlaceId
            });
            return Ok(Places);
        }
        [HttpGet]
        [Route("api/[controller]/IsNameUnique")]
        public ActionResult<bool> IsNameUnique(string name)
        {
            bool isUnique = !_context.Public_Place.AsNoTracking().Any(p => p.Name.ToLower() == name.ToLower());
            return Ok(isUnique);
        }
        
    }
}
