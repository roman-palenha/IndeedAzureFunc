using JobPostingIntegrationFunctions.Constants;

namespace JobPostingIntegrationFunctions.Models
{
    public class IntegrationSettingsDto
    {
        public string Name { get; set; }
        public JobPortal JobPortal { get; set; }
        public string Query { get; set; }
        public Localization Localization { get; set; }
        public string Location { get; set; }
        public int Page { get; set; }
    }
}
