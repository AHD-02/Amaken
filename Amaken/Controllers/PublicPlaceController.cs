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
        private readonly GoogleMapsGeocodingService _geocodingService;
        public Public_PlaceController(ApplicationDbContext context, NotificationController NotificationController,GoogleMapsGeocodingService geocodingService)
        {
            _context = context;
            _NotificationController = NotificationController;
            _geocodingService = geocodingService;
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
                var PublicPlace = _context.Public_Place.FirstOrDefault(u => u.PublicPlaceId!.Equals(id));
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
            var publicPlacesWithRates = _context.Public_Place
                .Select(place => new
                {
                    Place = place,
                    NumberOfRates = _context.PlacesRates.Count(r => r.PlaceId.ToLower().Equals(place.PublicPlaceId.ToLower())),
                    AverageScore = _context.PlacesRates.Where(r => r.PlaceId.ToLower().Equals(place.PublicPlaceId.ToLower())).Average(r => (double?)r.Score) ?? 0
                })
                .OrderByDescending(p => p.Place.AddedOn)
                .ToList();

            return Ok(publicPlacesWithRates);
        }
        [HttpGet]
        [Route("api/[controller]/SearchSavedPlaces")]
        [Authorize]
        public async Task<IActionResult> SearchSavedPlaces()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
                throw new UnauthorizedAccessException("User was not found");
            
            if (user.SavedPublicPlaces == null)
                return Ok(new List<Public_Place>());
            
            var publicPlaces = await _context.Public_Place
                .Where(e => user.SavedPublicPlaces.Contains(e.PublicPlaceId))
                .ToListAsync();

            return Ok(publicPlaces);
        }

        [HttpGet]
        [Route("api/[controller]/{id}/GetScore")]
        public IActionResult GetScore(string id)
        {
            return Ok(_context.PlacesRates.Where(u => u.PlaceId.Equals(id)).Average(u => u.Score));
        }
        
        
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult>GetPlace(string id)
        {
            var place = await _context.Public_Place
                .Where(e => e.PublicPlaceId == id)
                .Select(
                    place => new
                    {
                        Place = place,
                        NumberOfRates = _context.PlacesRates.Count(r => r.PlaceId.ToLower().Equals(place.PublicPlaceId.ToLower())),
                        AverageScore = _context.PlacesRates.Where(r => r.PlaceId.ToLower().Equals(place.PublicPlaceId.ToLower())).Average(r => (double?)r.Score) ?? 0

                    }
                    ).FirstOrDefaultAsync();
            if (place == null)
                throw new Exception($"Event with id {id} was not found");
            
            return Ok(place);
        }
           [HttpPost]
        [Route("api/[controller]/{placeId}/SavePlace")]
        [Authorize]
        public IActionResult SavePublicPlace(string placeId)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var myPlace = _context.Public_Place.FirstOrDefault(u => u.PublicPlaceId!.Equals(placeId));
                if (myPlace != null)
                {
                   
                    List<string> list = new List<string>(MyUser.SavedPublicPlaces ?? new string[0]);
                    if (!list.Contains(placeId))
                    {
                        list.Add(placeId);
                        MyUser.SavedPublicPlaces = list.ToArray();
                        _context.SaveChanges();
                        return Ok($"Place {placeId} is saved");
                    }

                    else
                    {
                        return BadRequest("Place is already saved");
                    } 
                }
                else
                {
                    return NotFound("Place wasn't found");
                }
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
        }
        [HttpPost]
        [Route("api/[controller]/{placeId}/UnSavePlace")]
        [Authorize]
        public IActionResult UnSavePlace(string placeId)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var myPlace = _context.Public_Place.FirstOrDefault(u => u.PublicPlaceId!.Equals(placeId));
                if (myPlace != null)
                {
                    List<string> list = new List<string>(MyUser.SavedPublicPlaces ?? new string[0]);
                    if (list.Contains(placeId))
                    {
                        list.Remove(placeId);
                        MyUser.SavedPublicPlaces = list.ToArray();
                        _context.SaveChanges();
                        return Ok($"Place {placeId} is removed");
                    }

                    else
                    {
                        return BadRequest("Place isn't saved");
                    } 
                }
                else
                {
                    return NotFound("Place wasn't found");
                }
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
        }
        [HttpGet("api/[controller]/GetCity")]
        public async Task<IActionResult> GetCity(double latitude, double longitude)
        {
            var city = await _geocodingService.GetCityAsync(latitude, longitude);
            if (city != null)
            {
                return Ok(new { City = city });
            }
            return NotFound("City not found");
        }
        [HttpGet]
        [Route("api/[controller]/IsNameUnique")]
        public ActionResult<bool> IsNameUnique(string name)
        {
            bool isUnique = !_context.Public_Place.Any(p => p.Name.ToLower() == name.ToLower());
            return Ok(isUnique);
        }
        
    }
}
