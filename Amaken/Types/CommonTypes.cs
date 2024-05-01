namespace Amaken.Types;
using System.Security.Cryptography;
using System.Text;
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


    public class LookupModel
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

}