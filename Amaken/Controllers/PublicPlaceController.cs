using Amaken.Models;
using Microsoft.AspNetCore.Mvc;

namespace Amaken.Controllers
{
    public class Public_PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public Public_PlaceController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("api/[controller]/CreatePublicPlace")]
        public IActionResult CreatePublicPlace(Public_Place myPublic_Place)
        {
            if (ModelState.IsValid)
            {
                myPublic_Place.AddedOn = DateTime.SpecifyKind(myPublic_Place.AddedOn, DateTimeKind.Utc);
                myPublic_Place.Status = "OK";
                _context.Public_Place.Add(myPublic_Place);
                _context.SaveChanges();
                return Ok("Public place was added successfully");
            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/TriggerPublicPlaceStatus")]
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
        [Route("api/[controller]/UpdatePublicPlace")]
        public IActionResult UpdatePublicPlace(Public_Place updatedPlace)
        {
            if (ModelState.IsValid)
            {
                var PublicPlace = _context.Public_Place.FirstOrDefault(u => u.PublicPlaceId!.Equals(updatedPlace.PublicPlaceId));
                if (PublicPlace != null)
                {
                    updatedPlace.AddedOn = DateTime.SpecifyKind(updatedPlace.AddedOn, DateTimeKind.Utc);
                    PublicPlace.UserEmail = updatedPlace.UserEmail;
                    PublicPlace.Location = updatedPlace.Location;
                    PublicPlace.Description = updatedPlace.Description;
                    PublicPlace.Status = updatedPlace.Status;
                    PublicPlace.Name = updatedPlace.Name;
                    PublicPlace.Images = updatedPlace.Images;
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
        [Route("api/[controller]/SearchPublicPlaces")]
        public IActionResult SearchPublicPlaces()
        {
            var PublicPlaces = _context.Public_Place.ToList();

            return Ok(PublicPlaces);
        }
    }
}
