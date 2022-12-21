using Newtonsoft.Json;

namespace JobPostingIntegrationFunctions.Models
{
    public class IndeedJobDetails
    {
        public string JobId { get; set; }
        [JsonProperty("company")]
        public IndeedJobCompany Company { get; set; }

        [JsonProperty("creation_date")]
        public string CreationDate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("indeed_final_url")]
        public string FinalUrl { get; set; }

        [JsonProperty("job_title")]
        public string Title { get; set; }

        [JsonProperty("job_type")]
        public object Type { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("salary")]
        public object Salary { get; set; }
    }
}
