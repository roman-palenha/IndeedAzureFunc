using FunctionApp1.Constants;
using FunctionApp1.Helpers;
using FunctionApp1.Models;
using FunctionApp1.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class IndeedJobSearch
    {
        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5" , RunOnStartup=true)]TimerInfo myTimer, ILogger log)
        {
            IOrganizationService service = Helper.Connection(log);
            if (service != null)
            {
                var uri = GetUri(service);
                var apiConfiguration = GetApiConfiguration(service);

                var serviceProvider = Startup.ConfigureIndeedServices();
                var searchJobService = serviceProvider.GetService<ISearchJobService>();

                var vacancies = await searchJobService.SendRequestAsync(uri, apiConfiguration);
                CreateVacancies(service, vacancies);
            }   
        }

        private static string GetUri(IOrganizationService service)
        {
            var integrationName = Environment.GetEnvironmentVariable("IntegrationSettings");
            var integrationColumns = new ColumnSet(IntegrationSettings.Name, IntegrationSettings.JobPortal, IntegrationSettings.Query, IntegrationSettings.Localization, IntegrationSettings.Location);
            var expr = new QueryExpression
            {
                EntityName = EntityName.IntegrationSettings,
                ColumnSet = integrationColumns
            };

            var integrationSettings  = service.RetrieveMultiple(expr)
                .Entities
                .FirstOrDefault(x => x.Attributes[IntegrationSettings.Name].ToString().Equals(integrationName)); 
            
            var query = Helper.CheckAndReplaceQuery(integrationSettings[IntegrationSettings.Query].ToString());
            var uri = JobSearch.Url + JobSearch.Query + $"{query}" + JobSearch.Location + $"{integrationSettings[IntegrationSettings.Location]}";

            if (((OptionSetValue)integrationSettings[IntegrationSettings.Localization]).Value != (int)Localization.DontIncludeLocalization)
                uri += JobSearch.Locality + $"{integrationSettings.FormattedValues[IntegrationSettings.Localization].ToLower()}";

            return uri;
        }

        private static ApiConfiguration GetApiConfiguration(IOrganizationService service)
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

        private static void CreateVacancies(IOrganizationService service, IEnumerable<IndeedHit> vacancies)
        {
            foreach (var v in vacancies)
            {
                var vacancyEntity = new Entity(EntityName.ColdLeads);
                vacancyEntity[ColdLead.Name] = v.Title;
                service.Create(vacancyEntity);
            }
        }
    }
}
