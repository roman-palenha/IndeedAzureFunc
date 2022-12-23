using Newtonsoft.Json;

namespace JobPostingIntegrationFunctions.Models
{
    public class CrmRequestBody
    {
        [JsonProperty("PrimaryEntityId")]
        public string PrimaryEntityId { get; set; }

        [JsonProperty("PrimaryEntityName")]
        public string PrimaryEntityName { get; set; }
    }
}
