namespace JobPostingIntegrationFunctions.Models
{
    public interface IIndeedApiConfiguration
    {
        string ApiHost { get; set; }
        string ApiKey { get; set; }
    }
}