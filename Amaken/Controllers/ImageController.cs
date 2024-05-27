using System.Security.Claims;
using Amaken.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

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
            public string fileExtension { get; set; }
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
                
                // Compress the image
                using (var inputStream = new MemoryStream(imageBytes))
                using (var image = Image.Load(inputStream))
                using (var outputStream = new MemoryStream())
                {
                    // Resize the image if needed (example: max width 800px)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(800, 0)
                    }));

                    // Save the compressed image to the output stream
                    image.Save(outputStream, new JpegEncoder
                    {
                        Quality = 75 // Adjust the quality setting as needed (0-100)
                    });

                    outputStream.Seek(0, SeekOrigin.Begin);

                    var fileName = Guid.NewGuid().ToString() + request.fileExtension;
                    var uploadedFileName = await _storageService.UploadImageAsync(outputStream, "image/jpeg", fileName);

                    _logger.LogInformation($"Uploaded file: {uploadedFileName}");

                    var imageUrl = $"{_endpointUrl}/{uploadedFileName}";
                    listOfImagesUrls.Add(imageUrl);
                }
            }

            string[] arrayOfImagesUrls = listOfImagesUrls.ToArray();
            return Ok(arrayOfImagesUrls);
        }

    }
}