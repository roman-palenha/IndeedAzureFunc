using System.Security.Cryptography;
using System.Text;

namespace JobPostingIntegrationFunctions.Models
{
    public class IndeedBlob
    {
        public string Description { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public string GetHash()
        {
            var input = Description + Title + Url;

            // Convert the input string to a byte array and compute the hash.
            byte[] data = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
