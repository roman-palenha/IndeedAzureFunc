using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions
{
    public static class IndeedJobSearch
    {
        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                var serviceProvider = Startup.ConfigureIndeedServices();
                var indeedJobService = serviceProvider.GetService<IIndeedJobService>();

                var blobStorageService = serviceProvider.GetService<IBlobStorageService>();

                
                var indeedJobDetails = new List<IndeedJobDetails>();

                await indeedJobService.GetJobsFromApi(blobStorageService, indeedJobDetails);

                var response = indeedJobService.CreateCrmJobs(indeedJobDetails);
                response.CheckFault(log);
                    
            }
            catch (Exception ex)
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
