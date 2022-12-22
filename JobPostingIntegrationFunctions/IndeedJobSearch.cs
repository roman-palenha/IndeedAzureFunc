using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Helpers;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions
{
    public static class IndeedJobSearch
    {

        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            IServiceProvider serviceProvider = Startup.ConfigureCRMServices();
            var service = serviceProvider.GetService<IOrganizationService>();
            if (service != null)
            {
                var indeedJobService = serviceProvider.GetService<IIndeedJobService>();

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
                        var existed = AzureHelper.GetRecordFromTable(job.Id);
                        if (existed == null)
                        {
                            jobDetails.JobId = job.Id;
                            indeedJobDetails.Add(jobDetails);
                            AzureHelper.InsertRecordToTable(job.Id, hash.ToString());
                        }
                    }
                }
                var response = indeedJobService.CreateCrmJobs(indeedJobDetails);
                response.CheckFault(log);

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
