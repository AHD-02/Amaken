using System.Security.Claims;
using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amaken.Controllers;

public class PlacesRatesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PlacesRatesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("api/[controller]/Rate")]
    public IActionResult CreatePublicPlaceCategory(int score, string PlaceID)
    {
        if (ModelState.IsValid)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                if(PlaceID.Split("-")[0].Equals("Public"))
                {
                    var MyPlace = _context.Public_Place.Where(u => u.PublicPlaceId.Equals(PlaceID)).FirstOrDefault();
                    if (MyPlace != null)
                    {
                        PlacesRates rate = new PlacesRates();
                        rate.UserEmail = MyUser.Email;
                        rate.PlaceId = MyPlace.PublicPlaceId;
                        rate.Score = score;
                        _context.PlacesRates.Add(rate);
                        _context.SaveChanges();
                        return Ok("Rate has been saved successfully");
                    }
                    else
                    {
                        return NotFound("Place wasn't found");
                    }
                }
                else if (PlaceID.Split("-")[0].Equals("Private"))
                {
                    var MyPlace = _context.Private_Place.Where(u => u.PlaceId.Equals(PlaceID)).FirstOrDefault();
                    if (MyPlace != null)
                    {
                        PlacesRates rate = new PlacesRates();
                        rate.UserEmail = MyUser.Email;
                        rate.PlaceId = MyPlace.PlaceId;
                        rate.Score = score;
                        _context.PlacesRates.Add(rate);
                        _context.SaveChanges();
                        return Ok("Rate has been saved successfully");
                    }
                    else
                    {
                        return NotFound("Place wasn't found");
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
        else
        {
            return BadRequest("Invalid data");
        }
    }


}