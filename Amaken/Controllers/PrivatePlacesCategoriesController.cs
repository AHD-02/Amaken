using Amaken.Models;
using Amaken.Types;

namespace Amaken.Controllers;
using Microsoft.AspNetCore.Mvc;

public class PrivatePlacesCategoriesController : Controller
{
    public readonly ApplicationDbContext _context;

    public PrivatePlacesCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    [Route("api/[controller]/Create")]
    public IActionResult CreatePrivatePlaceCategory([FromBody] PrivatePlacesCategories Category)
    {
        if (ModelState.IsValid)
        {
            _context.PrivatePlacesCategories.Add(Category);
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
        var Categories = _context.PrivatePlacesCategories.Select(category => new CommonTypes.LookupModel
        {
            Value = category.ID,
            Label = category.Name
        });
        return Ok(Categories);
    }
}