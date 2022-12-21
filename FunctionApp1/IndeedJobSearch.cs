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
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5" , RunOnStartup=false)]TimerInfo myTimer, ILogger log)
        {
            IOrganizationService service = Helper.Connection(log);
            if (service != null)
            {
                var uri = GetSearchUri(service);
                var apiConfiguration = IndeedHelper.GetApiConfiguration(service);

                var serviceProvider = Startup.ConfigureIndeedServices();
                var searchJobService = serviceProvider.GetService<ISearchJobService>();
                var getDetailsService = serviceProvider.GetService<IGetJobDetailsService>();

                var vacancies = await searchJobService.SendRequestAsync(uri, apiConfiguration);
                var details = new List<IndeedJobDetails>();
                log.LogInformation($"Pulled {vacancies.Count()} vacancies from Indeed");

                foreach(var v in vacancies)
                {
                    uri = GetDetailsUri(service, v.Id);
                    var detail = await getDetailsService.SendRequestAsync(uri, apiConfiguration);
                    if(detail.CreationDate != IndeedHitConstants.More30Days)
                    {
                        var indeedBlob = new IndeedBlob
                        {
                            Description = detail.Description,
                            Title = detail.Title,
                            Url = detail.FinalUrl
                        };
                        var hash = indeedBlob.GetHashCode();
                        var existed = AzureHelper.GetRecordFromTable(v.Id);
                        if(existed == null)
                        {
                            detail.JobId = v.Id;
                            details.Add(detail);
                            AzureHelper.InsertRecordToTable(v.Id, hash.ToString());
                        }
                    }  
                }

                var response = Helper.BulkCreate(service, details);
                response.CheckFault(log);
            }   
        }

        private static string GetSearchUri(IOrganizationService service)
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

        private static string GetDetailsUri(IOrganizationService service, string id)
        {
            var uri = JobDetails.Url + id;
            return uri;
        }

        private static void CheckFault(this OrganizationResponse ResponseItemObj, ILogger log)
        {
            foreach (KeyValuePair<string, object> results in ResponseItemObj.Results)
            {
                if (results.Value.GetType() == typeof(ExecuteMultipleResponseItemCollection))
                {
                    foreach (ExecuteMultipleResponseItem executeResp in (ExecuteMultipleResponseItemCollection)results.Value)
                    {
                        if (executeResp.Fault != null)
                        {
                            log.LogError(executeResp.Fault.Message);
                        }
                    }
                }
            }
        }
    }
}
