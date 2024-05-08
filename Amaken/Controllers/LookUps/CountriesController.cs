using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amaken.Controllers.Lookups
{
    public class CountriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CountriesController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {
            var countries = _context.Country.OrderBy(country => country.Name).Select(country => new CommonTypes.LookupModel
            {
                Label = country.Name,
                Value = country.Code
            });
        
            return Ok(countries);
        }
        
    }
}
