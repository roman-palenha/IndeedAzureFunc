using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;

namespace JobPostingIntegrationFunctions.Helpers
{
    public static class IndeedHelper
    {
        public static IEnumerable<IndeedHit> GetIndeedHitsFromResponse(string response)
        {
            var indeedResponse = JsonConvert.DeserializeObject<IndeedResponse>(response);
            return indeedResponse.Hits;
        }

        public static IndeedApiConfiguration GetApiConfiguration(IOrganizationService service)
        {
            var configurationName = Environment.GetEnvironmentVariable("ApiConfiguration");
            var configurationColumns = new ColumnSet(ConfigurationSettings.Name, ConfigurationSettings.RequestUrl, ConfigurationSettings.RapidHost, ConfigurationSettings.RapidKey);
            var expr = new QueryExpression
            {
                EntityName = EntityName.ConfigurationSettings,
                ColumnSet = configurationColumns
            };

            var configuration = service.RetrieveMultiple(expr)
                .Entities
                .FirstOrDefault(x => x.Attributes[ConfigurationSettings.Name].ToString().Equals(configurationName));

            var apiConfiguration = new IndeedApiConfiguration
            {
                ApiHost = configuration[ConfigurationSettings.RapidHost].ToString(),
                ApiKey = configuration[ConfigurationSettings.RapidKey].ToString()
            };

            return apiConfiguration;
        }

        public static Entity GetIntegrationSettings(IOrganizationService service)
        {
            var integrationName = Environment.GetEnvironmentVariable("IntegrationSettings");
            var integrationColumns = new ColumnSet(IntegrationSettings.Name, IntegrationSettings.JobPortal, IntegrationSettings.Query, IntegrationSettings.Localization, IntegrationSettings.Location, IntegrationSettings.NumberOfPages);
            var expr = new QueryExpression
            {
                EntityName = EntityName.IntegrationSettings,
                ColumnSet = integrationColumns
            };

            var integrationSettings = service.RetrieveMultiple(expr)
                .Entities
                .FirstOrDefault(x => x.Attributes[IntegrationSettings.Name].ToString().Equals(integrationName));

            return integrationSettings;
        }

        public static IndeedJobDetails GetJobDetails(string response)
        {
            var indeedResponse = JsonConvert.DeserializeObject<IndeedJobDetails>(response);
            return indeedResponse;
        }
    }
}
