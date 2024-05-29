using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Amaken.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly SendGridSettings _sendGridSettings;

        public AdminController(IOptions<SendGridSettings> sendGridSettings, ApplicationDbContext context,
            IConfiguration configuration)
        {
            _sendGridSettings = sendGridSettings.Value;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("api/[controller]/SignIn")]
        public IActionResult SignIn([FromBody] CommonTypes.SignInRequest request)
        {
            var Admin = _context.Admin.FirstOrDefault(u =>
                u.Email!.ToLower() == request.Email.ToLower() && u.Password.Equals(HashPassword(request.Password)));

            if (Admin != null)
            {
                if (Admin.Status == "OK")
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
        public IActionResult CreateAdmin([FromBody] Admin admin)
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
        public IActionResult UpdateAdmin([FromBody] Admin admin)
        {
            if (ModelState.IsValid)
            {
                var myAdmin = _context.Admin.FirstOrDefault(u => u.Email!.Equals(admin.Email));
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
                var admin = _context.Admin.FirstOrDefault(u => u.Email!.Equals(email));
                if (admin != null)
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
        public IActionResult ApprovePrivatePlace(string newPlaceId)
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

        [HttpPost]
        [Route("api/[controller]/SendApprovalEmailToOwner")]
        public async Task<IActionResult> SendApprovalEmailToOwner([FromBody] string placeID)
        {
            var place = _context.Private_Place.Where(u => u.PlaceId.ToLower().Equals(placeID.ToLower()))
                .FirstOrDefault();
            var user = _context.User.Where(u => u.Email.ToLower().Equals(place.UserEmail.ToLower())).FirstOrDefault();
            if (user != null && place != null)
            {
                var client = new SendGridClient(_sendGridSettings.ApiKey);
                Console.WriteLine($"API: {_sendGridSettings.ApiKey}");
                var from = new EmailAddress("amakenjo.team@gmail.com", "Amaken");
                var subject = string.Empty;
                var plainTextContent = string.Empty;
                var htmlContent = string.Empty;
                subject = "Your Registration Request Has Been Approved!";
                plainTextContent = $@"
Dear {user.FirstName},

We are delighted to inform you that your request to register your private place has been approved! Your place is now officially listed with us.

Details:
- Registered Place: {place.PlaceName}
- Registration Date: {place.AddedOn}

Thank you for choosing our platform. We are excited to have you on board and look forward to providing you with the best service possible.

Best regards,
The Amaken Team";

                htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
        }}
        .container {{
            width: 80%;
            margin: auto;
            padding: 20px;
            border: 1px solid #ccc;
            border-radius: 10px;
            background-color: #f9f9f9;
        }}
        .header {{
            text-align: center;
            padding: 10px 0;
        }}
        .content {{
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            padding: 10px 0;
            font-size: 12px;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Congratulations!</h1>
        </div>
        <div class='content'>
            <p>Dear <strong>{user.FirstName}</strong>,</p>
            <p>We are delighted to inform you that your request to register your private place has been <strong>approved</strong>! Your place is now officially listed with us.</p>
            <p>Details:</p>
            <ul>
                <li>Registered Place: <strong>{place.PlaceName}</strong></li>
                <li>Registration Date: <strong>{place.AddedOn}</strong></li>
            </ul>
            <p>Thank you for choosing our platform. We are excited to have you on board and look forward to providing you with the best service possible.</p>
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
                return NotFound("User or place wasn't found");
            }
        }

        public class RejectedPrivatePlace
        {
            public string PlaceID { get; set; }
            public string RejectionReason { get; set; }
        }

        [HttpPost]
        [Route("api/[controller]/SendRejectionEmailToOwner")]
        public async Task<IActionResult> SendRejectionEmailToOwner([FromBody] RejectedPrivatePlace RejectedPlace)
{
    var place = _context.Private_Place.Where(u => u.PlaceId.ToLower().Equals(RejectedPlace.PlaceID.ToLower()))
        .FirstOrDefault();
    var user = _context.User.Where(u => u.Email.ToLower().Equals(place.UserEmail.ToLower())).FirstOrDefault();

    if (user != null && place != null)
    {
        var client = new SendGridClient(_sendGridSettings.ApiKey);
        Console.WriteLine($"API: {_sendGridSettings.ApiKey}");
        var from = new EmailAddress("amakenjo.team@gmail.com", "Amaken");

        var subject = "Your Registration Request Has Been Declined";
        var plainTextContent = $@"
Dear {user.FirstName},

We regret to inform you that your request to register your private place has been declined after careful consideration.

Reason for rejection:
{RejectedPlace.RejectionReason}

If you have any questions or require further information, please do not hesitate to contact us.

Thank you for understanding.

Best regards,
The Amaken Team";

        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
        }}
        .container {{
            width: 80%;
            margin: auto;
            padding: 20px;
            border: 1px solid #ccc;
            border-radius: 10px;
            background-color: #f9f9f9;
        }}
        .header {{
            text-align: center;
            padding: 10px 0;
        }}
        .content {{
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            padding: 10px 0;
            font-size: 12px;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>We're Sorry</h1>
        </div>
        <div class='content'>
            <p>Dear <strong>{user.FirstName}</strong>,</p>
            <p>We regret to inform you that your request to register your private place has been <strong>declined</strong> after careful consideration.</p>
            <p><strong>Reason for rejection:</strong></p>
            <p>{RejectedPlace.RejectionReason}</p>
            <p>If you have any questions or require further information, please do not hesitate to contact us.</p>
            <p>Thank you for understanding.</p>
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
        return NotFound("User or place wasn't found");
    }
}


        public static string HashPassword(string password)
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