using Amaken.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Amaken.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
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
                        myReservation.DateOfReservation =
                            DateTime.SpecifyKind(myReservation.DateOfReservation, DateTimeKind.Utc);
                        myReservation.UserEmail = MyUser.Email;
                        myReservation.Status = "OK";
                        _context.Reservation.Add(myReservation);
                        _context.SaveChanges();
                        return Ok("Reservation is created.");
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
                        Reservation.DateOfReservation = DateTime.SpecifyKind(Reservation.DateOfReservation, DateTimeKind.Utc);
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
                    myReservation.DateOfReservation = DateTime.SpecifyKind(myReservation.DateOfReservation, DateTimeKind.Utc);
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
        [HttpGet]
        [Route("api/[controller]/SearchReservations")]
        public IActionResult SearchReservations()
        {
            var Reservations = _context.Reservation.ToList();

            return Ok(Reservations);
        }
    }
}
