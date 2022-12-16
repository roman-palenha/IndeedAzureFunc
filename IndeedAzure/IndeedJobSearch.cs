using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IndeedAzure
{
    public static class IndeedJobSearch
    {
        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5"
            , RunOnStartup=true
            )]TimerInfo myTimer, ILogger log)
        {
            IOrganizationService service = Helper.Connection(log);
            if (service != null)
            {
                var query = new QueryExpression("la_integrationsetting") { ColumnSet = new ColumnSet("la_name", "createdon", "la_indeedlocation", "la_query", "la_indeedlocalization") };
                var integrationSettings = service.RetrieveMultiple(query)[0];

                var uri = $"https://indeed12.p.rapidapi.com/jobs/search?query={integrationSettings["la_query"]}&location={integrationSettings["la_indeedlocation"]}";

                if (integrationSettings["la_indeedlocalization"] != null)
                    uri += $"&locality={integrationSettings["la_indeedlocalization"]}";

                query = new QueryExpression("la_apiconfiguration") { ColumnSet = new ColumnSet("la_name", "la_apiconfigurationid") };
                var configuration = service.RetrieveMultiple(query)[0];

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri),
                    Headers =
                    {
                        { "X-RapidAPI-Key", configuration["la_apiconfigurationid"].ToString() },
                        { "X-RapidAPI-Host", configuration["la_name"].ToString() },
                    },
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(body);
                }
            }
        }
    }
}
