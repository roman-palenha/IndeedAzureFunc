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
            var data = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(input));

            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
