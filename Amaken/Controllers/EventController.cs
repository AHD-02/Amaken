﻿using Amaken.Models;
using Microsoft.AspNetCore.Mvc;

namespace Amaken.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EventController (ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("api/[controller]/CreateEvent")]
        public IActionResult CreateEvent(Event newEvent)
        {
            if (ModelState.IsValid)
            {
                var user = _context.User.FirstOrDefault(u => u.Email!.ToLower() == newEvent.UserEmail!.ToLower());
                newEvent.EventStart = DateTime.SpecifyKind(newEvent.EventStart, DateTimeKind.Utc);
                newEvent.EventEnd = DateTime.SpecifyKind(newEvent.EventEnd, DateTimeKind.Utc);
                if (user != null) {_context.Event.Add(newEvent);
                    _context.SaveChanges();
                return Ok("Event is created"); }
                else { return NotFound("User wasn't found"); }
                
                
            } 
            else return BadRequest("Data is invalid");
        }
        [HttpPut]
        [Route("api/[controller]/UpdateEvent")]
        public IActionResult UpdateEvent(Event updatedEvent)
        {
            if (ModelState.IsValid)
            {
                updatedEvent.EventStart = DateTime.SpecifyKind(updatedEvent.EventStart, DateTimeKind.Utc);
                updatedEvent.EventEnd = DateTime.SpecifyKind(updatedEvent.EventEnd, DateTimeKind.Utc);
                var existingEvent = _context.Event.FirstOrDefault(u => u.EventId!.ToLower() == updatedEvent.EventId!.ToLower());
                if (existingEvent != null)
                {
                    existingEvent.Name = updatedEvent.Name;
                    existingEvent.Location = updatedEvent.Location;
                    existingEvent.EventType = updatedEvent.EventType;
                    existingEvent.Description = updatedEvent.Description;
                    existingEvent.EventStart = updatedEvent.EventStart;
                    existingEvent.EventEnd = updatedEvent.EventEnd;
                    existingEvent.Fees = updatedEvent.Fees;
                    existingEvent.UserEmail = updatedEvent.UserEmail;
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
        [HttpDelete]
        [Route("api/[controller]/DeleteEvent")]
        public IActionResult DeleteEvent(string EventId)
        {
            if (ModelState.IsValid)
            {
                var Event = _context.Event.FirstOrDefault(u => u.EventId!.ToLower().Equals(EventId.ToLower()));
                if (Event != null)
                {
                    Event.EventStart = DateTime.SpecifyKind(Event.EventStart, DateTimeKind.Utc);
                    Event.EventEnd = DateTime.SpecifyKind(Event.EventEnd, DateTimeKind.Utc);
                    _context.Event.Remove(Event);
                    _context.SaveChanges();
                    return Ok("Event was deleted successfully");
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

    }
}