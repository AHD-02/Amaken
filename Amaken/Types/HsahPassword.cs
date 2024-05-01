using System.Security.Cryptography;
using System.Text;
using Azure.Core;

namespace Amaken.Types
{
    public class HashPass
    {
        public static string HashPassword (string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        
                StringBuilder builder = new StringBuilder();
        
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }
        
                return builder.ToString();
            }
        }
    }
}


