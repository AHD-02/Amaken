using Microsoft.AspNetCore.Mvc;

namespace Amaken.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthCheckController: ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
    
    [HttpPost]
    public IActionResult Post()
    {
        return Ok("Posted");
    }
}