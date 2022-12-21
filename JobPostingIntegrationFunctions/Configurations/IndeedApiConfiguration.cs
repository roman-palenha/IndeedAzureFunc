namespace JobPostingIntegrationFunctions.Models
{
    public class IndeedApiConfiguration : IIndeedApiConfiguration
    {
        public string ApiKey { get; set; }
        public string ApiHost { get; set; }
    }
}
