using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Mvc;

namespace Amaken.Controllers;

public class PublicPlacesCategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PublicPlacesCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("api/[controller]/Create")]
    public IActionResult CreatePublicPlaceCategory([FromBody] PublicPlacesCategories Category)
    {
        if (ModelState.IsValid)
        {
            _context.PublicPlacesCategories.Add(Category);
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
        var Categories = _context.PublicPlacesCategories.Select(category => new CommonTypes.LookupModel
        {
            Value = category.ID,
            Label = category.Name
        });
        return Ok(Categories);
    }
}