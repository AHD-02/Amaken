using Amaken.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Amaken.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }
        public int GetLastId()
        {
            using (var context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
            {
                return context.Reservation
                    .AsEnumerable() 
                    .Where(x => int.TryParse(x.ReservationId, out _)) 
                    .OrderByDescending(x => int.Parse(x.ReservationId!)) 
                    .Select(x => int.Parse(x.ReservationId!)) 
                    .FirstOrDefault(); 
            }
        }
        
        [HttpPost]
        [Route("api/[controller]/CreateReservation")]
        [Authorize]
        public IActionResult CreateReservation([FromBody] Reservation myReservation)
        {
            if (ModelState.IsValid)
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
                if (MyUser != null)
                {
                    var Event = _context.Event.FirstOrDefault(u =>
                        u.EventId!.ToLower().Equals(myReservation.EventId!.ToLower()));
                    if (Event != null)
                    {
                        int lastId = GetLastId();
                        int newId = lastId + 1;
                        myReservation.ReservationId = $"{lastId + 1}";
                        myReservation.DateOfReservation =
                            DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                        myReservation.UserEmail = MyUser.Email;
                        myReservation.Status = "OK";
                        _context.Reservation.Add(myReservation);
                        _context.SaveChanges();
                        return Ok($"{newId}");
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
            else
            {
                return BadRequest("Invalid user data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/TriggerReservationStatus")]
        public IActionResult TriggerReservationStatus(string myReservationId, string newStatus)
        {
            if (ModelState.IsValid)
            {
                var Reservation = _context.Reservation.FirstOrDefault(u => u.ReservationId!.ToLower().Equals(myReservationId.ToLower()));
                if (Reservation != null)
                {
                    if (Enum.IsDefined(typeof(ReservationStatus), newStatus))
                    {
                        Reservation.DateOfReservation = DateTime.SpecifyKind((DateTime)Reservation.DateOfReservation, DateTimeKind.Utc);
                        Reservation.Status = newStatus;
                        _context.SaveChanges();
                        return Ok("Reservation status triggered to " + newStatus);
                    }
                    else
                    {
                        return BadRequest("Status is undefined");
                    }

                }
                else
                {
                    return NotFound("Reservation wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid user data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/UpdateReservation")]
        public IActionResult UpdateReservation([FromBody] Reservation myReservation)
        {
            if (ModelState.IsValid)
            {
                var Reservation = _context.Reservation.FirstOrDefault(u => u.ReservationId == myReservation.ReservationId);
                if (Reservation != null)
                {
                    myReservation.DateOfReservation = DateTime.SpecifyKind((DateTime)myReservation.DateOfReservation, DateTimeKind.Utc);
                    Reservation.UserEmail = myReservation.UserEmail;
                    Reservation.DateOfReservation = myReservation.DateOfReservation;
                    Reservation.EventId = myReservation.EventId;
                    Reservation.Status = myReservation.Status;
                    _context.SaveChanges();
                    return Ok("Reservation was updated successfully");
                }
                else
                {
                    return NotFound("Reservation wasn't found");
                }

            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [Authorize]
        [HttpGet]
        [Route("api/[controller]/SearchReservations")]
        public IActionResult SearchReservations()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            var Reservations = _context.Reservation.Where(u=>u.UserEmail.Equals(MyUser.Email)).Join(_context.Event, reservation => reservation.EventId,
                @event => @event.EventId,
                (reservation, @event) => new
                {
                    reservation.ReservationId,
                    reservation.DateOfReservation,
                    reservation.UserEmail,
                    reservation.EventId,
                    reservation.Status,
                    @event.Name,
                    @event.EventStart,
                    @event.EventEnd,
                    @event.EventType,
                    @event.Description,
                    @event.Images,
                    @event.Fees,
                    @event.PlaceID
                }
            ).Join(_context.Public_Place,
                combined=>combined.PlaceID,
                place =>place.PublicPlaceId,
                (combined, place) => new
                {
                    
                    combined.ReservationId,
                    combined.DateOfReservation,
                    combined.UserEmail,
                    combined.EventId,
                    combined.Status,
                    EventName = combined.Name,
                    combined.EventStart,
                    combined.EventEnd,
                    combined.EventType,
                    combined.Description,
                    combined.Images,
                    combined.Fees,
                    PlaceName = place.Name,
                })
                .ToList();
            return Ok(Reservations);
        }
        [HttpGet]
        [Route("api/[controller]/GetReservation")]
        public IActionResult GetReservation(string id)
        {
            var reservationsWithEventAndPlaceDetails =  _context.Reservation.Where(u=>u.ReservationId.Equals(id))
                .Join(_context.Event,
                    reservation => reservation.EventId,
                    @event => @event.EventId,
                    (reservation, @event) => new
                    {
                        reservation.ReservationId,
                        reservation.EventId,
                        reservation.UserEmail,
                        reservation.DateOfReservation,
                        reservation.Status,
                        EventImages = @event.Images,
                        EventName = @event.Name,
                        @event.EventStart,
                        @event.EventEnd,
                        @event.PlaceID  
                    })
                .Join(_context.Public_Place,
                    combined => combined.PlaceID,
                    place => place.PublicPlaceId,
                    (combined, place) => new
                    {
                        combined.ReservationId,
                        combined.EventId,
                        combined.UserEmail,
                        combined.DateOfReservation,
                        combined.Status,
                        combined.EventImages,
                        combined.EventName,
                        combined.EventStart,
                        combined.EventEnd,
                        PlaceName = place.Name
                    })
                .FirstOrDefault();
            if (reservationsWithEventAndPlaceDetails == null)
            {
                reservationsWithEventAndPlaceDetails =  _context.Reservation.Where(u=>u.ReservationId.Equals(id))
                    .Join(_context.Event,
                        reservation => reservation.EventId,
                        @event => @event.EventId,
                        (reservation, @event) => new
                        {
                            reservation.ReservationId,
                            reservation.EventId,
                            reservation.UserEmail,
                            reservation.DateOfReservation,
                            reservation.Status,
                            EventImages = @event.Images,
                            EventName = @event.Name,
                            @event.EventStart,
                            @event.EventEnd,
                            @event.PlaceID  
                        })
                    .Join(_context.Private_Place,
                        combined => combined.PlaceID,
                        place => place.PlaceId,
                        (combined, place) => new
                        {
                            combined.ReservationId,
                            combined.EventId,
                            combined.UserEmail,
                            combined.DateOfReservation,
                            combined.Status,
                            combined.EventImages,
                            combined.EventName,
                            combined.EventStart,
                            combined.EventEnd,
                            PlaceName = place.PlaceName
                        })
                    .FirstOrDefault();
            }

            return Ok(reservationsWithEventAndPlaceDetails);


        }
    }
}
