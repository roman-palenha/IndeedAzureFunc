namespace FunctionApp1.Constants
{
    public static class EntityName
    {
        public const string IntegrationSettings = "la_integrationsetting";
        public const string ConfigurationSettings = "la_apiconfiguration";
        public const string ColdLeads = "la_coldlead";
    }

    public static class IntegrationSettings
    {
        public const string Name = "la_name";
        public const string JobPortal = "la_jobsportal";
        public const string Query = "la_query";
        public const string Localization = "la_indeedlocalization";
        public const string Location = "la_indeedlocation";
    }

    public static class ConfigurationSettings
    {
        public const string Name = "la_name";
        public const string RequestUrl = "la_requesturl";
        public const string RapidHost = "la_xrapidapihost";
        public const string RapidKey = "la_xrapidapikey";
    }

    public static class ColdLead
    {
        public const string Name = "la_name";
        public const string ExternalId = "la_externalid";
        public const string Url = "la_url";
        public const string Description = "crc14_description";
        public const string CreatedOn = "la_postcreatedon";
    }

    public static class RequestHeaders
    {
        public const string RapidHost = "X-RapidAPI-Host";
        public const string RapidKey = "X-RapidAPI-Key";
    }

    public static class JobSearch
    {
        public const string Url = "https://indeed12.p.rapidapi.com/jobs/search";
        public const string Query = "?query=";
        public const string Location = "&location=";
        public const string Locality = "&locality=";
    }

    public static class JobDetails
    {
        public const string Url = "https://indeed12.p.rapidapi.com/job/";
    }

    public static class IndeedHitConstants
    {
        public const string More30Days = "30+ days ago";
        public const string JustPosted = "Just Posted";
    }

    public static class AzureTable
    {
        public const string IndeedJobs = "IndeedJobs";
    }
}
