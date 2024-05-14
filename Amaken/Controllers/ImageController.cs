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
            public string Name { get; set; }
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
                    var fileName = await _storageService.UploadImageAsync(stream, "image/jpeg", request.Name);

                    _logger.LogInformation($"Uploaded file: {fileName}");

                    var imageUrl = $"{_endpointUrl}/{fileName}";

                    listOfImagesUrls.Add(imageUrl);
                }
            }

            string[] arrayOfImagesUrls = listOfImagesUrls.ToArray();
            return Ok(arrayOfImagesUrls);
        }
    }
}