using System.Security.Claims;
using Amaken.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Amaken.Controllers
{
    public class ImageController : ControllerBase
    {
        private readonly DigitalOceanStorageService _storageService;
        private readonly string _endpointUrl;
        private readonly ILogger<ImageController> _logger;
        private readonly ApplicationDbContext _context; 
        public ImageController(DigitalOceanStorageService storageService, 
            IOptions<DigitalOceanSpacesSettings> settings,
            ILogger<ImageController> logger,ApplicationDbContext context)
        {
            _storageService = storageService;
            _endpointUrl = "amaken-images.fra1.digitaloceanspaces.com";
            _logger = logger;
            _context = context;
        }
        public class Base64UploadRequest
        {
            public string Base { get; set; }
        }
        [HttpPost]
        [Route("api/[controller]/UploadImage")]
        public async Task<IActionResult> UploadImage([FromBody] Base64UploadRequest[] base64Requests)
        {
            if (base64Requests == null || base64Requests.Length == 0)
            {
                _logger.LogWarning("No base64 images received.");
                return BadRequest("No base64 images received");
            }

            List<string> listOfImagesUrls = new List<string>();

            foreach (var request in base64Requests)
            {
                _logger.LogInformation("UploadImage method called.");

                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(request.Base);
                }
                catch (FormatException ex)
                {
                    _logger.LogError($"Invalid base64 string: {ex.Message}");
                    return BadRequest($"Invalid base64 string: {ex.Message}");
                }

                using (var stream = new MemoryStream(imageBytes))
                {
                    string fileExtension = GetFileExtension(imageBytes);

                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var UploadedFileName = await _storageService.UploadImageAsync(stream, "image/jpeg", fileName);

                    _logger.LogInformation($"Uploaded file: {UploadedFileName}");

                    var imageUrl = $"{_endpointUrl}/{UploadedFileName}";

                    listOfImagesUrls.Add(imageUrl);
                }
            }

            string[] arrayOfImagesUrls = listOfImagesUrls.ToArray();
            return Ok(arrayOfImagesUrls);
        }
        private string GetFileExtension(byte[] fileBytes)
        {
            if (fileBytes.Length < 4)
            {
                throw new ArgumentException("Invalid file data.");
            }

            if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF)
            {
                return ".jpg"; 
            }
            else if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47)
            {
                return ".png"; 
            }
            else if (fileBytes[0] == 0x47 && fileBytes[1] == 0x49 && fileBytes[2] == 0x46)
            {
                return ".gif";
            }
            else
            {
                throw new ArgumentException("Unsupported file type.");
            }
        }
    }
}