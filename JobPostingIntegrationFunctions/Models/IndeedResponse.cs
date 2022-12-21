using Newtonsoft.Json;
using System.Collections.Generic;

namespace JobPostingIntegrationFunctions.Models
{
    public class IndeedResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("hits")]
        public List<IndeedHit> Hits { get; set; }
        [JsonProperty("indeed_final_url")]
        public string FinalUrl { get; set; }
        [JsonProperty("suggest_locality")]
        public object SuggestLocality { get; set; }
    }
}
