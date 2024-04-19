using Amaken.Models;
using Microsoft.AspNetCore.Mvc;

namespace Amaken.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("api/[controller]/CreateAdmin")]
        public IActionResult CreateAdmin (Admin admin)
        {
            if (ModelState.IsValid)
            {
                admin.Status = "OK";
                _context.Admin.Add(admin);
                _context.SaveChanges();
                return Ok("Admin was created successfully");

            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/UpdateAdmin")]
        public IActionResult UpdateAdmin (Admin admin)
        {
            if (ModelState.IsValid)
            {
                var myAdmin = _context.Admin.FirstOrDefault(u=>u.Email!.Equals(admin.Email));
                if (myAdmin != null)
                {
                    myAdmin.Password = admin.Password;
                    myAdmin.Name = admin.Name;
                    myAdmin.Status = admin.Status;
                    _context.SaveChanges();
                    return Ok("Admin was updated successfully");
                }
                else
                {
                    return NotFound("Admin wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/TriggerAdminStatus")]
        public IActionResult TriggerAdminStatus(string email, string status)
        {
            if (ModelState.IsValid)
            {
               var admin = _context.Admin.FirstOrDefault(u=>u.Email!.Equals(email));
                if(admin != null)
                {
                    if (Enum.IsDefined(typeof(AdminStatus), status))
                    {
                        admin.Status = status;
                        _context.SaveChanges();
                        return Ok("Admin status was triggered successfully");
                    }
                    else
                    {
                        return BadRequest("Status isn't defined");
                    }
                }
                else
                {
                    return NotFound("Admin wasn't found");
                }
            }
            else
            {
                return BadRequest("Invalid Data");
            }
        }
        [HttpPut]
        [Route("api/[controller]/ApprovePrivatePlace")]
        public IActionResult ApprovePrivatePlace (string newPlaceId)
        {
            if (ModelState.IsValid)
            {
                var PrivatePlace = _context.Private_Place.FirstOrDefault(u => u.PlaceId!.Equals(newPlaceId));
                if (PrivatePlace != null)
                {
                    PrivatePlace.Status = "OK";
                    _context.SaveChanges();
                    return Ok("Private place is approved");
                }
                else
                {
                    return NotFound("Private place wasn't found");
                }
            }
            else
            {
                return BadRequest("Data is invalid");
            }
        }
        [HttpGet]
        [Route("api/[controller]/SearchAdmins")]
        public IActionResult SearchAdmins()
        {
            var Admins = _context.Admin.ToList();

            return Ok(Admins);
        }
    }
}
