using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services
{
    public class IndeedJobService : IIndeedJobService
    {
        private readonly ICrmService crmService;
        private readonly IHttpRequestService httpRequestService;
        private readonly IIndeedApiConfiguration apiConfiguration;

        public IndeedJobService(ICrmService crmService, IHttpRequestService httpRequestService)
        {
            this.crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
            this.httpRequestService = httpRequestService;
            this.apiConfiguration = GetApiConfiguration();
        }

        public async Task<IndeedJobDetails> GetJobDetails(string id)
        {
            var request = CreateHttpRequest(JobDetails.Url + id);
            return await httpRequestService.ExecuteGetRequest<IndeedJobDetails>(request);
        }

        public async Task<IEnumerable<IndeedHit>> GetJobs()
        {
            var result = new List<IndeedHit>();
            var integrationSettings = crmService.GetIntegrationSettings();
            foreach (var item in integrationSettings)
            {
                var numberOfPages = item.GetAttributeValue<int>(IntegrationSettings.NumberOfPages);

                for (int i = 1; i < numberOfPages; i++)
                {
                    var uri = CreateUri(item);
                    uri += JobSearch.Page + i.ToString();

                    var request = CreateHttpRequest(uri);
                    var response = await httpRequestService.ExecuteGetRequest<IndeedResponse>(request);
                    result.AddRange(response.Hits);
                }
            }

            return result;
        }

        public OrganizationResponse CreateCrmJobs(IEnumerable<IndeedJobDetails> jobDetails)
        {
            return crmService.BulkCreate(jobDetails);
        }

        private IndeedApiConfiguration GetApiConfiguration()
        {
            return crmService.GetApiConfiguration();
        }

        private HttpRequestMessage CreateHttpRequest(string uri)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers =
                {
                     { RequestHeaders.RapidKey, apiConfiguration.ApiKey },
                     { RequestHeaders.RapidHost, apiConfiguration.ApiHost },
                },
            };

            return request;
        }

        private string CreateUri(Entity item)
        {
            var query = item.GetAttributeValue<string>(IntegrationSettings.Query).Replace(" ", "%20");
            var location = item.GetAttributeValue<string>(IntegrationSettings.Location);
            var uri = JobSearch.Url + JobSearch.Query + $"{query}" + JobSearch.Location + $"{location}";

            if (((OptionSetValue)item[IntegrationSettings.Localization]).Value != (int)Localization.DontIncludeLocalization)
                uri += JobSearch.Locality + $"{item.FormattedValues[IntegrationSettings.Localization].ToLower()}";
            return uri;
        }


    }
}
