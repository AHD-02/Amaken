using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;

namespace Amaken.Controllers;

public class OpenAIController : Controller
{
    [HttpPost]
    [Route("api/[controller]/EnhanceDescription")]
    public async Task<IActionResult> EnhanceDescription([FromBody] string UserDescription)
    {
        {
            string apiKey = "sk-proj-SUIoRc5VevdI0DbgfnRuT3BlbkFJNtzMFSwFCL2jsv7eyyeV";
            string answer = string.Empty;
            var openai = new OpenAIAPI(apiKey);
            CompletionRequest completion = new CompletionRequest
            {
                Prompt = $"Original Description:\n{UserDescription}\n\nRegenerated Description:",
                Model = OpenAI_API.Models.Model.ChatGPTTurboInstruct,
                MaxTokens = 4000
            };

            var result = await openai.Completions.CreateCompletionAsync(completion);
            if (result != null && result.Completions.Count > 0)
            {
                answer = result.Completions[0].Text;
                return Ok(new { generatedDescription = answer });
            }
            else
            {
                return BadRequest(new { error = "Not found" });
            }
        }
    }
    
    [HttpPost]
    [Route("api/[controller]/GenerateEventImages")]
    public IActionResult GenerateEventImages([FromBody] string UserDescription)
    {
        string apiKey = "sk-proj-SUIoRc5VevdI0DbgfnRuT3BlbkFJNtzMFSwFCL2jsv7eyyeV";
        string answer = string.Empty;
        var openai = new OpenAIAPI(apiKey);
        CompletionRequest completion = new CompletionRequest();
        completion.Prompt = UserDescription;
        completion.Model = OpenAI_API.Models.Model.DALLE3;
        completion.MaxTokens = 4000;
        var result = openai.Completions.CreateCompletionAsync(completion);
        if (result != null)
        {
            foreach (var item in result.Result.Completions)
            {
                answer = item.Text;
            }
            return Ok(answer);
        }
        else
        {
            return BadRequest("Not found");
        }
    }
}