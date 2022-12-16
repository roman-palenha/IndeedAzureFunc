using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1.Models
{
    public class IndeedHit
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("company_name")]
        public string Company { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("locality")]
        public string Locality { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("zip")]
        public string Zip { get; set; }
    }
}
