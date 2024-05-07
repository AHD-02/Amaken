using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
namespace Amaken.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; 
        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost]
        [Route("api/[controller]/SignIn")]
        public IActionResult SignIn([FromBody] CommonTypes.SignInRequest request)
        {
            var Admin = _context.Admin.FirstOrDefault(u => u.Email!.ToLower() == request.Email.ToLower() && u.Password.Equals(HashPassword(request.Password)));

            if (Admin != null)
            {
                if (Admin.Status=="OK")
                {
                    var jwtSecret = _configuration["Jwt:Secret"];
                    var token = UserController.GenerateJwtToken(request.Email!, jwtSecret);

                    return Ok(new { Token = token });
                }
                else
                {
                    return BadRequest("Admin account isn't accessible");
                }
            }
            else
            {
                return Unauthorized("Invalid email or password");
            }
        }
        [HttpPost]
        [Route("api/[controller]/Create")]
        public IActionResult CreateAdmin ([FromBody] Admin admin)
        {
            if (ModelState.IsValid)
            {
                admin.Status = "OK";
                admin.Password = HashPassword(admin.Password);
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
        public IActionResult UpdateAdmin ([FromBody] Admin admin)
        {
            if (ModelState.IsValid)
            {
                var myAdmin = _context.Admin.FirstOrDefault(u=>u.Email!.Equals(admin.Email));
                if (myAdmin != null)
                {
                    myAdmin.Password = HashPassword(admin.Password!);
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
        
        [HttpGet]
        [Route("api/[controller]/{email}")]
        public IActionResult GetAdmin(string email)
        {
            var admin = _context.Admin.Where(a => a.Email == email).ToList();
            
            if (admin == null)
            {
                return BadRequest("Admin Not Found");
            }
            
            return Ok(admin);
        }
        public static string HashPassword (string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        
                StringBuilder builder = new StringBuilder();
        
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }
        
                return builder.ToString();
            }
        }
    }
}
