using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1.Models
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
