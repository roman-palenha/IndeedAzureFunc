using FunctionApp1.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class IndeedJobSearch
    {
        private const string DoNotIncludeLocal = "Do not include in request";
        private const string _uri = "https://indeed12.p.rapidapi.com/jobs/search?query=";

        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5" , RunOnStartup=true)]TimerInfo myTimer, ILogger log)
        {
            IOrganizationService service = Helper.Connection(log);
            if (service != null)
            {
                var integrationSettings = service.Retrieve("la_integrationsetting", new Guid("06709d5f-a57c-ed11-81ac-002248d73072"), new ColumnSet("la_name", "la_jobsportal", "la_query", "la_indeedlocalization", "la_indeedlocation"));

                var query = Helper.CheckAndReplaceQuery(integrationSettings["la_query"].ToString());
                var uri = _uri + $"{query}&location={integrationSettings["la_indeedlocation"]}";

                if (!integrationSettings["la_indeedlocalization"].ToString().Equals(DoNotIncludeLocal))
                    uri += $"&locality={integrationSettings.FormattedValues["la_indeedlocalization"].ToString().ToLower()}";

                var configuration = service.Retrieve("la_apiconfiguration", new Guid("2bd2c8b3-a17c-ed11-81ac-002248d73072"), new ColumnSet("la_name", "la_requesturl", "la_xrapidapihost", "la_xrapidapikey"));


                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri),
                    Headers =
                    {
                        { "X-RapidAPI-Key", configuration["la_xrapidapikey"].ToString() },
                        { "X-RapidAPI-Host", configuration["la_xrapidapihost"].ToString() },
                    },
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var vacancies = Helper.GetIndeedHitsFromResponse(body);
                    CreateVacancies(service, vacancies);
                }
            }   
        }

        private static void CreateVacancies(IOrganizationService service, IEnumerable<IndeedHit> vacancies)
        {
            foreach(var v in vacancies)
            {
                var vacancyEntity = new Entity("la_coldlead");
                vacancyEntity["la_name"] = v.title;
                service.Create(vacancyEntity);
            }
        }
    }
}
