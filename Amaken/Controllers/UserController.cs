using Microsoft.AspNetCore.Mvc;
using Amaken.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Amaken.Types;

namespace Amaken.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public UserController(ApplicationDbContext context) 
        {
            _context = context;
        }
        
        [HttpPost]
        [Route("api/[controller]/CreateUser")]
        public IActionResult CreateUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (CheckEmail(newUser.Email!) == "OK")
                {
                    newUser.DateOfBirth = DateTime.SpecifyKind(newUser.DateOfBirth, DateTimeKind.Utc);
                    newUser.Status = "OK";
                    _context.User.Add(newUser);
                    _context.SaveChanges();
                    var token = GenerateJwtToken(newUser.Email!);
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
        [Route("api/[controller]/UpdateUser")]
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
                    User.Password = newUser.Password;
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
            var user = _context.User.FirstOrDefault(u => u.Email!.ToLower() == request.Email.ToLower() && u.Password == request.Password);

            if (user != null)
            {
                if (user.Status == "OK")
                {
                    var token = GenerateJwtToken(user.Email!);

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

        static public string GenerateJwtToken(string userEmail)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var randomBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            var base64Secret = Convert.ToBase64String(randomBytes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(base64Secret));

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
        [Route("api/[controller]/SearchUsers")]
        public IActionResult SearchUsers()
        {
            var Users = _context.User.ToList();

            return Ok(Users);
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
