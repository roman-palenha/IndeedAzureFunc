using FunctionApp1.Constants;
using FunctionApp1.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionApp1.Helpers
{
    public static class IndeedHelper
    {
        public static IEnumerable<IndeedHit> GetIndeedHitsFromResponse(string response)
        {
            var indeedResponse = JsonConvert.DeserializeObject<IndeedResponse>(response);
            return indeedResponse.Hits;
        }

        public static ApiConfiguration GetApiConfiguration(IOrganizationService service)
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

            var apiConfiguration = new ApiConfiguration
            {
                ApiHost = configuration[ConfigurationSettings.RapidHost].ToString(),
                ApiKey = configuration[ConfigurationSettings.RapidKey].ToString()
            };

            return apiConfiguration;
        }

        public static IndeedJobDetails GetJobDetails(string response)
        {
            var indeedResponse = JsonConvert.DeserializeObject<IndeedJobDetails>(response);
            return indeedResponse;
        }
    }
}
