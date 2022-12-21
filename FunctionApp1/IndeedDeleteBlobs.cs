using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Services.Description;
using FunctionApp1.Constants;
using FunctionApp1.Helpers;
using FunctionApp1.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunctionApp1
{
    public static class IndeedDeleteBlobs
    {
        [FunctionName("IndeedDeleteBlobs")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, ILogger log)
        {

            var content = await req.Content.ReadAsStringAsync();
            var myDeserializedClass = JsonConvert.DeserializeObject<Root>(content);

            var id = myDeserializedClass.PrimaryEntityId;

            log.LogInformation($"{id}");

            var service = Helper.Connection(log);
            var coldLeadsColumns = new ColumnSet(ColdLead.Name, ColdLead.Url, ColdLead.Description, ColdLead.ExternalId, ColdLead.CreatedOn);
            var expr = new QueryExpression
            {
                EntityName = EntityName.ColdLeads,
                ColumnSet = coldLeadsColumns
            };

            var coldLeads = service.RetrieveMultiple(expr)
                .Entities;
            try
            {
                var records = AzureHelper.GetRecordsFromTable();

                foreach (var r in records)
                {
                    if (!coldLeads.Any(x => x[ColdLead.ExternalId].ToString() == r.Id))
                        AzureHelper.DeleteRecordFromTable(r.Id);
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex.Message);
            }
            
           

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
