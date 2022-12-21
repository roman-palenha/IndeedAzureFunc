using Newtonsoft.Json;

namespace FunctionApp1.Models
{
    public class IndeedJobCompany
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("indeed_absolute_link")]
        public string AboluteLink { get; set; }

        [JsonProperty("indeed_relative_link")]
        public string RelativeLink { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("locality")]
        public string Locality { get; set; }

        [JsonProperty("logo_url")]
        public string LogoUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
