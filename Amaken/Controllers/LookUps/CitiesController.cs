using Amaken.Models;
using Amaken.Types;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Amaken.Controllers.Lookups
{
    public class CitiesController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get(string countryCode)
        {
            if (countryCode == "")
            {
                return BadRequest();
            }
    
            var cities = _context.City.Where(c => c.Country_Code == countryCode)
                .Select(country => new CommonTypes.LookupModel
                {
                    Label = country.Name,
                    Value = country.ID.ToString()
                });

            return Ok(cities);
        }


    }
}
