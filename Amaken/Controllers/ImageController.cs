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
        public async Task<IActionResult> UploadImage(IFormFile[] files)
        {
            List <string> ListOfImages = new List<string>();
            
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
    }
}