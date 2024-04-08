using Amaken.Models;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult CreateReservation(Reservation myReservation)
        {
            if (ModelState.IsValid)
            {
                var User = _context.User.FirstOrDefault(u => u.Email!.ToLower() == myReservation.UserEmail!.ToLower());
                var Event = _context.Event.FirstOrDefault(u => u.EventId!.ToLower() == myReservation.EventId!.ToLower());

                if (User != null)
                {
                    if (Event != null)
                    {
                        myReservation.DateOfReservation = DateTime.SpecifyKind(myReservation.DateOfReservation, DateTimeKind.Utc);
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
                    return NotFound("User wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid user data");
            }
        }
        [HttpDelete]
        [Route("api/[controller]/DeleteReservation")]
        public IActionResult DeleteReservation(string myReservationId)
        {
            if (ModelState.IsValid)
            {
                var Reservation = _context.Reservation.FirstOrDefault(u=>u.ReservationId!.ToLower().Equals(myReservationId.ToLower()));
                if(Reservation != null)
                {
                Reservation.DateOfReservation = DateTime.SpecifyKind(Reservation.DateOfReservation, DateTimeKind.Utc);
                    _context.Reservation.Remove(Reservation);
                    _context.SaveChanges();
                    return Ok("Reservation was deleted successfully");
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
        public IActionResult UpdateReservation(Reservation myReservation)
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
