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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Amaken.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;  
        private readonly IConfiguration _configuration; 
        private readonly SendGridSettings _sendGridSettings;
        
        public UserController(IOptions<SendGridSettings> sendGridSettings,ApplicationDbContext context, IConfiguration configuration) 
        {
            _context = context;
            _configuration = configuration;
            _sendGridSettings = sendGridSettings.Value;
        }
                [HttpPost]
        [Route("api/[controller]/SendWelcomeEmail")]
public async Task<IActionResult> SendWelcomeEmail([FromBody] string userEmail)
{
    var user = _context.User.Where(u => u.Email.ToLower().Equals(userEmail.ToLower())).FirstOrDefault();
    if (user != null)
    {
        var client = new SendGridClient(_sendGridSettings.ApiKey);
        var from = new EmailAddress("amakenjo.team@gmail.com", "Amaken");
        var subject = "Welcome to Amaken!";
        var plainTextContent = $@"
Dear {user.FirstName},

Welcome to Amaken!

We are thrilled to have you join our community. At Amaken, we are dedicated to providing you with the best experience possible. Whether you are here to explore new places or share your own, we are excited to have you with us.

If you have any questions, feel free to reach out to us at any time. We are here to help!

Best regards,
The Amaken Team";

        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 80%;
            margin: auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            padding: 20px 0;
        }}
        .content {{
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px 0;
            font-size: 12px;
            color: #888888;
        }}
        img {{
            max-width: 200px;
            height: auto;
            display: block;
            margin: 0 auto 20px auto;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://amaken-images.fra1.digitaloceanspaces.com/Amaken%20Logo.png' alt='Amaken Logo' />
            <h1>Welcome to Amaken!</h1>
        </div>
        <div class='content'>
            <p>Dear <strong>{user.FirstName}</strong>,</p>
            <p>Welcome to <strong>Amaken</strong>! We are thrilled to have you join our community.</p>
            <p>At Amaken, we are dedicated to providing you with the best experience possible. Whether you are here to explore new places or share your own, we are excited to have you with us.</p>
            <p>If you have any questions, feel free to reach out to us at any time. We are here to help!</p>
        </div>
        <div class='footer'>
            <p>Best regards,</p>
            <p>The Amaken Team</p>
        </div>
    </div>
</body>
</html>";

        var to = new EmailAddress(user.Email, $"{user.FirstName} {user.LastName}");
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
        var responseBody = await response.Body.ReadAsStringAsync();
        return Ok(new { statusCode = response.StatusCode, body = responseBody });
    }
    else
    {
        return NotFound("User not found");
    }
}

        [HttpPost]
        [Route("api/[controller]/Create")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto newUser)
        {
            if (ModelState.IsValid)
            {
                if (CheckEmail(newUser.Email!) == "OK")
                {
                    newUser.DateOfBirth = DateTime.SpecifyKind(newUser.DateOfBirth, DateTimeKind.Utc);
                    newUser.Status = "OK";
                    newUser.Password = HashPassword(newUser.Password!);
                    User myNewUser = new User(newUser);
                    myNewUser.Discriminator = "User";
                    _context.User.Add(myNewUser);
                    _context.SaveChanges();
                    var jwtSecret = _configuration["Jwt:Secret"];
                    var token = GenerateJwtToken(newUser.Email!, jwtSecret);
                    await SendWelcomeEmail(myNewUser.Email);
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
        [HttpPost]
        [Route("api/[controller]/Update")]
        public IActionResult UpdateUser([FromBody] User newUser)
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
                    User.Intrests = newUser.Intrests;
                    User.SavedEvents = newUser.SavedEvents;
                    User.SavedPublicPlaces = newUser.SavedPublicPlaces;
                    User.Discriminator = newUser.Discriminator;
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
                Expires = DateTime.UtcNow.AddYears(1),
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

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/MyPublicPlaces")]
        public IActionResult MyPublicPlaces()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var MyPlaces = _context.Public_Place.Where(u => u.UserEmail!.Equals(userEmail)).ToList();
                MyPlaces = MyPlaces.OrderByDescending(e => e.AddedOn).ToList();
                return Ok(MyPlaces);
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
            
        }
        [HttpGet]
        [Authorize]
        [Route("api/[controller]/MyNotifications")]
        public IActionResult MyNotifications()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                var MyNotifications = _context.Notification.Where(u => u.UserEmail!.Equals(MyUser.Email));
                return Ok(MyNotifications);
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
            
        }
        [HttpGet]
        [Authorize]
        [Route("api/[controller]/MyEvents")]
        public IActionResult MyEvents(string status)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            if (MyUser != null)
            {
                if (status.ToLower().Equals("running"))
                {
                    var MyEvents = 
                        _context.Event.Where(u => u.EventStart.CompareTo(DateTime.Now) <= 0 
                        && u.EventEnd.CompareTo(DateTime.Now)>=0
                        ).ToList();
                    MyEvents = MyEvents.OrderByDescending(e => e.CreatedOn).ToList();
                    return Ok(MyEvents);
                }
                else if (status.ToLower().Equals("upcoming"))
                {
                    var MyEvents = 
                        _context.Event.Where(u => u.EventStart.CompareTo(DateTime.Now) > 0 
                                                  && u.EventEnd.CompareTo(DateTime.Now) > 0
                        ).ToList();
                    MyEvents = MyEvents.OrderByDescending(e => e.CreatedOn).ToList();
                    return Ok(MyEvents);
                }else if (status.ToLower().Equals("ended"))
                {
                    var MyEvents = 
                        _context.Event.Where(u => u.EventStart.CompareTo(DateTime.Now) < 0 
                                                  && u.EventEnd.CompareTo(DateTime.Now)<0
                        ).ToList();
                    MyEvents = MyEvents.OrderByDescending(e => e.CreatedOn).ToList();
                    return Ok(MyEvents);
                }
                else
                {
                    return BadRequest("Invalid request");
                }
            }
            else
            {
                return Unauthorized("User isn't authorized");
            }
            
        }
        [HttpGet]
        [Authorize]
        [Route("api/[controller]/MyAllEvents")]
        public IActionResult MyAllEvents()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            
            if (MyUser != null)
            {
                var MyEvents = _context.Event.Where(u => u.UserEmail!.Equals(userEmail)).ToList();
                MyEvents = MyEvents.OrderByDescending(e => e.CreatedOn).ToList();

                return Ok(MyEvents);
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
