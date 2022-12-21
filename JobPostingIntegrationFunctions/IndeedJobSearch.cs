using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Helpers;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace JobPostingIntegrationFunctions
{
    public static class IndeedJobSearch
    {
        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            IOrganizationService service = Helper.Connection(log);
            if (service != null)
            {
                var serviceProvider = Startup.ConfigureIndeedServices();
                var searchJobService = serviceProvider.GetService<ISearchJobService>();
                var getDetailsService = serviceProvider.GetService<IGetJobDetailsService>();
                var integrationSettings = IndeedHelper.GetIntegrationSettings(service);
                var pages = int.Parse(integrationSettings[IntegrationSettings.NumberOfPages].ToString());
                for(int i = 1; i <= pages; ++i)
                {
                    await SearchJobsAsync(service, searchJobService, getDetailsService, log, i);
                }
               
            }
        }

        private static async Task SearchJobsAsync(IOrganizationService service, ISearchJobService searchJobService, IGetJobDetailsService getDetailsService, ILogger log, int page)
        {
            var uri = GetSearchUri(service, page);
            var apiConfiguration = IndeedHelper.GetApiConfiguration(service);

            var vacancies = await searchJobService.SendRequestAsync(uri, apiConfiguration);
            var details = new List<IndeedJobDetails>();
            log.LogInformation($"Pulled {vacancies.Count()} vacancies from Indeed");

            foreach (var v in vacancies)
            {
                uri = GetDetailsUri(service, v.Id);
                var detail = await getDetailsService.SendRequestAsync(uri, apiConfiguration);
                if (detail.CreationDate != IndeedHitConstants.More30Days)
                {
                    var indeedBlob = new IndeedBlob
                    {
                        Description = detail.Description,
                        Title = detail.Title,
                        Url = detail.FinalUrl
                    };
                    var hash = indeedBlob.GetHashCode();
                    var existed = AzureHelper.GetRecordFromTable(v.Id);
                    if (existed == null)
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
    

        private static string GetSearchUri(IOrganizationService service, int page)
        {
            var integrationSettings = IndeedHelper.GetIntegrationSettings(service);
            var query = Helper.CheckAndReplaceQuery(integrationSettings[IntegrationSettings.Query].ToString());
            var uri = JobSearch.Url + JobSearch.Query + $"{query}" + JobSearch.Location + $"{integrationSettings[IntegrationSettings.Location]}";

            if (((OptionSetValue)integrationSettings[IntegrationSettings.Localization]).Value != (int)Localization.DontIncludeLocalization)
                uri += JobSearch.Locality + $"{integrationSettings.FormattedValues[IntegrationSettings.Localization].ToLower()}";

            uri += JobSearch.Page + page.ToString();

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
