using System.ComponentModel.DataAnnotations;

namespace Amaken.Types;

public class CommonTypes
{
    public class SignInRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class PagedModel<T>
    {
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public IEnumerable<T> Matches { get; set; }
    }

    public class City
    {
        [Key]
        [Required]
        public string ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Country_Code { get; set; }

        public City(string name, string MyCountryCode)
        {
            this.Name = name;
            this.Country_Code = MyCountryCode;
        }
    }
}