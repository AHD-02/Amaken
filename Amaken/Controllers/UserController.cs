using Microsoft.AspNetCore.Mvc;
using Amaken.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Amaken.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context; // Declare ApplicationDbContext field
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration) // Inject ApplicationDbContext
        {
            _context = context; // Initialize ApplicationDbContext field
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Route("api/[controller]/CreateUser")]
        public IActionResult CreateUser(User newUser) {    
            if (ModelState.IsValid)
            {
                    newUser.DateOfBirth = DateTime.SpecifyKind(newUser.DateOfBirth, DateTimeKind.Utc);
                    _context.User.Add(newUser);
                    _context.SaveChanges();
                var token = GenerateJwtToken(newUser.Email!);
                return Ok(new { Token = token });
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
                    User.Image = newUser.Image;
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
        public IActionResult SignIn(string email, string password)
        {
            var user = _context.User.FirstOrDefault(u => u.Email!.ToLower() == email.ToLower() && u.Password == password);

            if (user != null)
            {
                var token = GenerateJwtToken(user.Email!);

                return Ok(new { Token = token });
            }
            else
            {
                return Unauthorized("Invalid email or password");
            }
        }
        [HttpDelete]
        [Route("api/[controller]/DeleteUser")]
        public IActionResult DeleteUser(string Email)
        {
            if (ModelState.IsValid) {var User = _context.User.FirstOrDefault(u => u.Email!.ToLower().Equals(Email.ToLower()));
            if(User != null)
            {
                _context.User.Remove(User);
                _context.SaveChanges();
                return Ok("User was deleted successfully");
            }
            else {
                return NotFound("User wasn't found");
            } }
            else
            {
                return BadRequest("Data is invalid");
            }
            
        }

        static private string GenerateJwtToken(string userEmail)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var randomBytes = new byte[32]; // 32 bytes = 256 bits
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
                    // Add additional claims as needed
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time (adjust as needed)
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
        static string CheckPassword (string password)
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
        }
        static string CheckEmail (string email) { 

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
