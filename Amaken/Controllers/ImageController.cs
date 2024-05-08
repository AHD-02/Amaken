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

        [HttpPost]
        [Route("api/[controller]/UploadImage")]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile[] files)
        {
            
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var MyUser = _context.User.FirstOrDefault(u => u.Email!.Equals(userEmail));
            List <string> ListOfImages = new List<string>();
            if (MyUser != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    _logger.LogInformation("UploadImage method called.");
                
                    if (files[i] == null || files[i].Length == 0)
                    {
                        _logger.LogWarning("No file uploaded.");
                        return BadRequest("No file uploaded");
                    }
                
                    _logger.LogInformation($"Received file: {files[i].FileName}");
                
                    var fileName = await _storageService.UploadImageAsync(files[i]);
                
                    _logger.LogInformation($"Uploaded file: {fileName}");
                
                    var imageUrl = $"{_endpointUrl}/{fileName}";
                
                    ListOfImages.Add(imageUrl);
                }

                string[] ArrayOfImagesUrls = ListOfImages.ToArray();
                return Ok(ArrayOfImagesUrls);

            }
            else
            {
                return Unauthorized("User isn't authorized");
            }

        }
    }
}