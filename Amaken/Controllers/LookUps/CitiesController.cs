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

        /*[HttpPost]
        [Route("api/[controller]/AddCities")]
        public IActionResult AddCities([FromBody] string[] cities)
        {
                for (int i =0; i <cities.Length; i++)
                {
                    City myCity = new City();
                    myCity.ID = i+1;
                    myCity.Country_Code = "8";
                    myCity.Name = cities[i];
                    _context.City.Add(myCity);
                    _context.SaveChanges();
                }

                return Ok();
        }*/

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
