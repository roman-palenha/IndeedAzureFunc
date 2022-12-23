using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                var serviceProvider = Startup.ConfigureIndeedServices();
                var indeedJobService = serviceProvider.GetService<IIndeedJobService>();

                var jsonContent = await req.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<CrmRequestBody>(jsonContent);
                var deleteId = indeedJobService.GetColdLeadExternalId(content.PrimaryEntityId);

                var blobStorageService = serviceProvider.GetService<IBlobStorageService>();

                var records = blobStorageService.GetRecordsFromTable();
                if (records.Any(x => x.Id.Equals(deleteId, StringComparison.InvariantCultureIgnoreCase)))
                {
                    blobStorageService.DeleteRecordFromTable(deleteId);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }
    }
}
