using System.Security.Cryptography;
using System.Text;

namespace ChatneyBackend.Utils;

public static class Helpers
{
    public static string GetMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert to hex string
            StringBuilder sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // lowercase hex
            }
            return sb.ToString();
        }
    }
}