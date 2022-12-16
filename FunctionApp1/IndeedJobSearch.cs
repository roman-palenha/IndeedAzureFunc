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
using System.Web.Services.Description;

namespace FunctionApp1
{
    public static class IndeedJobSearch
    {
        private const string DoNotIncludeLocal = "Do not include in request";
        private const string _uri = "https://indeed12.p.rapidapi.com/jobs/search?query=";
        private const string _apiKey = "X-RapidAPI-Key";
        private const string _apiHost = "X-RapidAPI-Host";

        [FunctionName("IndeedJobSearch")]
        public static async Task Run([TimerTrigger("* 0 7 * * 1-5" , RunOnStartup=true)]TimerInfo myTimer, ILogger log)
        {
            IOrganizationService service = Helper.Connection(log);
            if (service != null)
            {
                var uri = GetUri(service);
                var apiConfiguration = GetApiConfiguration(service);

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri),
                    Headers =
                    {
                        { _apiKey, apiConfiguration.ApiKey },
                        { _apiHost, apiConfiguration.ApiHost },
                    },
                };

                await SendRequestAsync(client, request, service);
            }   
        }

        private static string GetUri(IOrganizationService service)
        {
            var integrationSettings = service.Retrieve("la_integrationsetting", new Guid("06709d5f-a57c-ed11-81ac-002248d73072"), new ColumnSet("la_name", "la_jobsportal", "la_query", "la_indeedlocalization", "la_indeedlocation"));

            var query = Helper.CheckAndReplaceQuery(integrationSettings["la_query"].ToString());
            var uri = _uri + $"{query}&location={integrationSettings["la_indeedlocation"]}";

            if (!integrationSettings.FormattedValues["la_indeedlocalization"].ToString().Equals(DoNotIncludeLocal))
                uri += $"&locality={integrationSettings.FormattedValues["la_indeedlocalization"].ToString().ToLower()}";

            return uri;
        }

        private static ApiConfiguration GetApiConfiguration(IOrganizationService service)
        {
            var configuration = service.Retrieve("la_apiconfiguration", new Guid("2bd2c8b3-a17c-ed11-81ac-002248d73072"), new ColumnSet("la_name", "la_requesturl", "la_xrapidapihost", "la_xrapidapikey"));
            var apiConfiguration = new ApiConfiguration { ApiHost = configuration["la_xrapidapihost"].ToString(), ApiKey = configuration["la_xrapidapikey"].ToString() };

            return apiConfiguration;
        }

        private static async Task SendRequestAsync(HttpClient client, HttpRequestMessage request, IOrganizationService service)
        {
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var vacancies = Helper.GetIndeedHitsFromResponse(body);
                CreateVacancies(service, vacancies);
            }
        }

        private static void CreateVacancies(IOrganizationService service, IEnumerable<IndeedHit> vacancies)
        {
            foreach (var v in vacancies)
            {
                var vacancyEntity = new Entity("la_coldlead");
                vacancyEntity["la_name"] = v.title;
                service.Create(vacancyEntity);
            }
        }
    }
}
