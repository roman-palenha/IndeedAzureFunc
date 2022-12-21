using FunctionApp1.Constants;
using FunctionApp1.Helpers;
using FunctionApp1.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class IndeedDeleteBlobs
    {
        [FunctionName("IndeedDeleteBlobs")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")] HttpRequestMessage req, ILogger log)
        {
            var jsonContent = await req.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<CrmRequestBody>(jsonContent);

            var entityId = content.PrimaryEntityId;

            var service = Helper.Connection(log);
            var coldLeadsColumns = new ColumnSet(ColdLead.Name, ColdLead.Url, ColdLead.Description, ColdLead.ExternalId, ColdLead.CreatedOn);
            var expr = new QueryExpression
            {
                EntityName = EntityName.ColdLeads,
                ColumnSet = coldLeadsColumns
            };

            var deleteEntity = service.Retrieve(EntityName.ColdLeads, new Guid(entityId), coldLeadsColumns);

            var records = AzureHelper.GetRecordsFromTable();

            var deleteId = deleteEntity[ColdLead.ExternalId].ToString();
            if (records.Any(x => x.Id == deleteId))
                AzureHelper.DeleteRecordFromTable(deleteId);
            else
                log.LogInformation($"Can not find a record with id {deleteId}");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
