using Amaken.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Amaken.Controllers
{
    public class ImageController : ControllerBase
    {
        private readonly DigitalOceanStorageService _storageService;
        private readonly string _endpointUrl;
        private readonly ILogger<ImageController> _logger;

        public ImageController(DigitalOceanStorageService storageService, 
            IOptions<DigitalOceanSpacesSettings> settings,
            ILogger<ImageController> logger)
        {
            _storageService = storageService;
            _endpointUrl = "amaken-images.fra1.digitaloceanspaces.com";
            _logger = logger;
        }

        [HttpPost]
        [Route("api/[controller]/UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            _logger.LogInformation("UploadImage method called.");

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded");
            }

            _logger.LogInformation($"Received file: {file.FileName}");

            var fileName = await _storageService.UploadImageAsync(file);

            _logger.LogInformation($"Uploaded file: {fileName}");

            var imageUrl = $"{_endpointUrl}/{fileName}";

            return Ok(new { imageUrl });
        }
    }
}