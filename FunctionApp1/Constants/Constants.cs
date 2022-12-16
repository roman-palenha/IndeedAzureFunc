using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public const string Description = "la_description";
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
}
