using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions
{
    public static class IndeedDeleteBlobs
    {
        [FunctionName("IndeedDeleteBlobs")]
        public static async Task Run([HttpTrigger(WebHookType = "genericJson")] HttpRequestMessage req, ILogger log)
        {
            try
            {
                var serviceProvider = Startup.ConfigureCRMServices();
                var service = serviceProvider.GetService<IOrganizationService>();
                if (service != null)
                {
                    serviceProvider = Startup.ConfigureIndeedServices(service);
                    var indeedJobService = serviceProvider.GetService<IIndeedJobService>();

                    var jsonContent = await req.Content.ReadAsStringAsync();
                    var deleteId = indeedJobService.GetColdLeadExternalId(jsonContent);

                    serviceProvider = Startup.ConfigureAzureServices();
                    var blobStorageService = serviceProvider.GetService<IBlobStorageService>();

                    var records = blobStorageService.GetRecordsFromTable();
                    if (records.Any(x => x.Id.Equals(deleteId, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        blobStorageService.DeleteRecordFromTable(deleteId);
                    }
                }
            } catch(Exception ex)
            {
                log.LogError(ex.Message);
            }
        }
    }
}
