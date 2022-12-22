using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions
{
    public static class IndeedJobSearch
    {

        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                var serviceProvider = Startup.ConfigureCRMServices();
                var service = serviceProvider.GetService<IOrganizationService>();
                if (service != null)
                {
                    serviceProvider = Startup.ConfigureIndeedServices(service);
                    var indeedJobService = serviceProvider.GetService<IIndeedJobService>();

                    serviceProvider = Startup.ConfigureAzureServices();
                    var blobStorageService = serviceProvider.GetService<IBlobStorageService>();

                    var jobs = await indeedJobService.GetJobs();
                    var indeedJobDetails = new List<IndeedJobDetails>();
                    foreach (var job in jobs)
                    {
                        var jobDetails = await indeedJobService.GetJobDetails(job.Id);
                        if (jobDetails.CreationDate != IndeedHitConstants.More30Days)
                        {
                            var indeedBlob = new IndeedBlob
                            {
                                Description = jobDetails.Description,
                                Title = jobDetails.Title,
                                Url = jobDetails.FinalUrl
                            };
                            var hash = indeedBlob.GetHashCode();
                            var existed = blobStorageService.GetRecordFromTable(job.Id);
                            if (existed == null)
                            {
                                jobDetails.JobId = job.Id;
                                indeedJobDetails.Add(jobDetails);
                                blobStorageService.InsertRecordToTable(job.Id, hash.ToString());
                            }
                        }
                    }
                    var response = indeedJobService.CreateCrmJobs(indeedJobDetails);
                    response.CheckFault(log);
                } 
            } catch(Exception ex)
            {
                log.LogError(ex.Message);
            }
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
