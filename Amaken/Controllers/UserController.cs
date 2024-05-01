using Microsoft.AspNetCore.Mvc;
using Amaken.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amaken.Types;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace Amaken.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;  
        private readonly IConfiguration _configuration; 
        
        public UserController(ApplicationDbContext context, IConfiguration configuration) 
        {
            _context = context;
            _configuration = configuration;
        }
        
        [HttpPost]
        [Route("api/[controller]/Create")]
        public IActionResult CreateUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (CheckEmail(newUser.Email!) == "OK")
                {
                    newUser.DateOfBirth = DateTime.SpecifyKind(newUser.DateOfBirth, DateTimeKind.Utc);
                    newUser.Status = "OK";
                    newUser.Password = HashPassword(newUser.Password!);
                    _context.User.Add(newUser);
                    _context.SaveChanges();
                    var jwtSecret = _configuration["Jwt:Secret"];
                    var token = GenerateJwtToken(newUser.Email!, jwtSecret);
                    return Ok(new { Token = token });
                }
                else
                {
                    return BadRequest("Email format is invalid");
                }

            }
            else
            {
                return BadRequest("Invalid user data");
            }

        }
        [HttpPut]
        [Route("api/[controller]/Update")]
        public IActionResult UpdateUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                newUser.DateOfBirth = DateTime.SpecifyKind(newUser.DateOfBirth, DateTimeKind.Utc);
                var User = _context.User.FirstOrDefault(u => u.Email!.ToLower().Equals(newUser.Email!.ToLower()));
                if (User != null)
                {
                    User.FirstName = newUser.FirstName;
                    User.LastName = newUser.LastName;
                    User.Password = HashPassword(newUser.Password!);
                    User.Phone = newUser.Phone;
                    User.Country = newUser.Country;
                    User.City = newUser.City;
                    User.DateOfBirth = newUser.DateOfBirth;
                    User.Images = newUser.Images;
                    User.Status = newUser.Status;
                    _context.SaveChanges();
                    return Ok("User has been updated");
                }
                else
                {
                    return NotFound("User was not found");
                }
            }
            else
            {
                return BadRequest("Invalid user data");
            }

        }
        [HttpPost]
        [Route("api/[controller]/SignIn")]
        public IActionResult SignIn([FromBody] CommonTypes.SignInRequest request)
        {
            var user = _context.User.FirstOrDefault(u => u.Email!.ToLower().Equals(request.Email.ToLower()) && u.Password!.Equals(HashPassword(request.Password)));

            if (user != null)
            {
                if (user.Status == "OK")
                {
                    var jwtSecret = _configuration["Jwt:Secret"];
                    var token = GenerateJwtToken(user.Email!, jwtSecret);

                    return Ok(new { Token = token });
                }
                else
                {
                    return BadRequest("User account isn't accessible");
                }
            }
            else
            {
                return Unauthorized("Invalid email or password");
            }
        }
        public class CityObject
        {
            public string value { get; set; }
            public string label { get; set; }
        }
        
        
        [HttpPost]
        [Route("api/[controller]/SaveEvent")]
        [Authorize]
        public IActionResult SaveEvent(string eventId)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var myEvent = _context.Event.FirstOrDefault(u => u.EventId!.Equals(eventId));
                if (myEvent != null)
                {
                   
                List<string> list = new List<string>(MyUser.SavedEvents ?? new string[0]);
                if (!list.Contains(eventId))
                {
                list.Add(eventId);
                MyUser.SavedEvents = list.ToArray();
                _context.SaveChanges();
                return Ok($"Event {eventId} is saved");
                }

                else
                {
                    return BadRequest("Event is already saved");
                } 
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
        [HttpPost]
        [Route("api/[controller]/UnSaveEvent")]
        [Authorize]
        public IActionResult UnSaveEvent(string eventId)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var myEvent = _context.Event.FirstOrDefault(u => u.EventId!.Equals(eventId));
                if (myEvent != null)
                {
                    List<string> list = new List<string>(MyUser.SavedEvents ?? new string[0]);
                    if (list.Contains(eventId))
                    {
                        list.Remove(eventId);
                        MyUser.SavedEvents = list.ToArray();
                        _context.SaveChanges();
                        return Ok($"Event {eventId} is removed");
                    }

                    else
                    {
                        return BadRequest("Event isn't saved");
                    } 
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
        
        [HttpPut]
        [Route("api/[controller]/TriggerUserStatus")]
        public IActionResult TriggerUserStatus (string email, string newStatus)
        {
            var User = _context.User.FirstOrDefault(u => u.Email!.ToLower().Equals(email.ToLower()));
            if(User != null)
            {
                if(Enum.IsDefined(typeof(UserStatus), newStatus))
                {
                    User.Status=newStatus;
                    _context.SaveChanges();
                    return Ok("User status triggered to " + newStatus);
                }
                else
                {
                    return BadRequest("Status is undefined");
                }
            }
            else
            {
                return BadRequest("User wasn't found");
            }
        }

        static public string GenerateJwtToken(string userEmail, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, userEmail),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        
        [HttpGet]
        [Route("api/[controller]/Me")]
        [Authorize]
        public IActionResult Me()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                return Ok(MyUser);
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
           
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
        
        /*static string CheckPassword(string password)
        {
            if (String.IsNullOrEmpty(password))
            {
                return "Password cannot be empty";
            }
            else if (password.Length < 8)
            {
                return "Password length cannot be less than 8";
            }
            else if (!password.Any(char.IsUpper))
            {
                return "Password must contain at least one upper character";
            }

            else if (!password.Any(char.IsLower))
            {
                return "Password must contain at least one lower character";
            }
            else if (!password.Any(char.IsDigit))
            {
                return "Password must contain at least one number";
            }

            return "OK";
        }*/

        static string CheckEmail(string email)
        {

            bool isValidEmail = Regex.IsMatch(email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
            
            if (isValidEmail)
            {
                return "OK";
            }
            else
            {
                return "Please check the email format";
            }
        }
    }
}
