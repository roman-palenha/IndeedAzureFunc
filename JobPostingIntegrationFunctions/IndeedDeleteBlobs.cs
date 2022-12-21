using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Helpers;
using JobPostingIntegrationFunctions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions
{
    public static class IndeedDeleteBlobs
    {
        [FunctionName("IndeedDeleteBlobs")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")] HttpRequestMessage req, ILogger log)
        {
            var jsonContent = await req.Content.ReadAsStringAsync();

            var service = Helper.Connection(log);
            if (service == null)
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var deleteId = GetColdLeadExternalId(service, jsonContent, log);
            var records = AzureHelper.GetRecordsFromTable();

            if (records.Any(x => x.Id == deleteId))
            {
                AzureHelper.DeleteRecordFromTable(deleteId);
            }
            else
            {
                log.LogWarning($"Not found a record with id {deleteId}");
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static string GetColdLeadExternalId(IOrganizationService service, string body, ILogger log)
        {
            var content = JsonConvert.DeserializeObject<CrmRequestBody>(body);
            var entityId = content.PrimaryEntityId;

            var coldLeadsColumns = new ColumnSet(ColdLead.Name, ColdLead.Url, ColdLead.Description, ColdLead.ExternalId, ColdLead.CreatedOn);
            var deleteEntity = service.Retrieve(EntityName.ColdLeads, new Guid(entityId), coldLeadsColumns);
            var deleteId = deleteEntity[ColdLead.ExternalId].ToString();
            log.LogInformation($"Pulled record with external id: {deleteId}");

            return deleteId;
        }
    }
}
