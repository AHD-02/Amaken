using System.Security.Claims;
using Amaken.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amaken.Controllers
{
    public class Private_PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public Private_PlaceController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("api/[controller]/CreatePriavtePlace")]
        [Authorize]
        public IActionResult CreatePriavtePlace(Private_Place myPrivate_Place)
        {
            if (ModelState.IsValid)
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
                if (MyUser != null)
                {
                    
                myPrivate_Place.AddedOn = DateTime.SpecifyKind(myPrivate_Place.AddedOn, DateTimeKind.Utc);
                myPrivate_Place.Status = "Unapproved";
                _context.Private_Place.Add(myPrivate_Place);
                _context.SaveChanges();
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
        public IActionResult UpdatePriavtePlace(Private_Place updatedPlace)
        {
            if (ModelState.IsValid)
            {
                var PrivatePlace = _context.Private_Place.FirstOrDefault(u => u.PlaceId!.Equals(updatedPlace.PlaceId));
                if (PrivatePlace != null)
                {
                    updatedPlace.AddedOn = DateTime.SpecifyKind(updatedPlace.AddedOn, DateTimeKind.Utc);
                    PrivatePlace.UserEmail = updatedPlace.UserEmail;
                    PrivatePlace.RegisterNumber= updatedPlace.RegisterNumber;
                    PrivatePlace.Location=updatedPlace.Location;
                    PrivatePlace.Description=updatedPlace.Description;
                    PrivatePlace.Status=updatedPlace.Status;
                    PrivatePlace.PlaceName=updatedPlace.PlaceName;
                    PrivatePlace.Images=updatedPlace.Images;
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
            var PrivatePlaces = _context.Private_Place.ToList();

            return Ok(PrivatePlaces);
        }
    }
}
