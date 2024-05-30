using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using OpenAI_API.Completions;

namespace Amaken.Controllers;

public class OpenAIController : Controller
{ 
    private readonly string _openAiApiKey = "sk-proj-SUIoRc5VevdI0DbgfnRuT3BlbkFJNtzMFSwFCL2jsv7eyyeV";
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DigitalOceanStorageService _DigitalOceanStorageService;
    private readonly string _bucketName;
    private readonly string _endpointUrl;

    public OpenAIController(ILogger<OpenAIController> logger,IHttpClientFactory httpClientFactory, DigitalOceanStorageService digitalOceam)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _DigitalOceanStorageService = digitalOceam;
        _bucketName = "amaken-images";
        _endpointUrl = "https://amaken-images.fra1.digitaloceanspaces.com";
    }

    public class EnhancedDescription
    {
        public string prompt { get; set; }
    }
    [HttpPost]
    [Route("api/[controller]/EnhanceDescription")]
    public async Task<IActionResult> EnhanceDescription([FromBody] EnhancedDescription request)
    {
        {
            string apiKey = "sk-proj-SUIoRc5VevdI0DbgfnRuT3BlbkFJNtzMFSwFCL2jsv7eyyeV";
            string answer = string.Empty;
            var openai = new OpenAIAPI(apiKey);
            CompletionRequest completion = new CompletionRequest
            {
                Prompt = $"Original Description:\n{request.prompt}\n\nRegenerated Description (short and concise):",
                Model = OpenAI_API.Models.Model.ChatGPTTurboInstruct,
                MaxTokens = 150
            };

            var result = await openai.Completions.CreateCompletionAsync(completion);
            if (result != null && result.Completions.Count > 0)
            {
                answer = result.Completions[0].Text;
                return Ok(answer);
            }
            else
            {
                return BadRequest(new { error = "Not found" });
            }
        }
    }
    
    [HttpPost]
    [Route("api/[controller]/GenerateImage")]
    
    public async Task<IActionResult> GenerateImage([FromBody] string prompt)
    {
        try
        {
            var imageUrl = await GenerateImageAsync(prompt);
            var lastImageUrl = await UploadImage(imageUrl);
            return Ok(lastImageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating the image.");
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost]
    [Route("api/[controller]/UploadImage")]
    public async Task<IActionResult> UploadImage([FromBody] string imageUrl)
    {
        try
        {
            var lastImageUrl = await DownloadAndUploadImage(imageUrl);

            return Ok(lastImageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading the image.");
            return StatusCode(500, ex.Message);
        }
    }

    private async Task<string> GenerateImageAsync(string prompt)
    {
        var requestBody = new JObject
        {
            { "prompt", prompt },
            { "n", 1 },
            { "size", "1024x1024" }
        };

        var content = new StringContent(requestBody.ToString(), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/images/generations", content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseString);
            var imageUrl = responseJson["data"][0]["url"].ToString();
            return imageUrl;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to generate image: {response.StatusCode}, {errorContent}");
            throw new Exception($"Failed to generate image: {response.StatusCode}");
        }
    }
    public async Task<IActionResult> DownloadAndUploadImage(string imageUrl)
    {
        try
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                using (var stream = new MemoryStream(imageBytes))
                {
                    var fileName = Guid.NewGuid().ToString() + ".jpg"; 
                    var uploadedFileName = await _DigitalOceanStorageService.UploadImageAsync(stream, "image/jpeg", fileName);
        
                    var uploadedImageUrl = $"{_endpointUrl}/{uploadedFileName}";

                    return Ok(uploadedImageUrl);
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to download and upload image: " + ex.Message);
        }
    }
    
}

