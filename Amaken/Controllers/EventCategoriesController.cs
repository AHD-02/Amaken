using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Mvc;

namespace Amaken.Controllers;

public class EventCategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EventCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("api/[controller]/Create")]
    public IActionResult CreateEventCategory([FromBody] EventCategories Category)
    {
        if (ModelState.IsValid)
        {
            _context.EventCategories.Add(Category);
            _context.SaveChanges();
            return Ok("Category is saved successfully");
        }
        else
        {
            return BadRequest("Invalid date");
        }
    }
    [HttpGet]
    [Route("api/[controller]/GetCategories")]
    public IActionResult GetCategories()
    {
        var Categories = _context.EventCategories.Select(category => new CommonTypes.LookupModel
        {
            Value = category.ID,
            Label = category.Name
        });
        return Ok(Categories);
    }
}