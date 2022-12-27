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
            this.httpRequestService = httpRequestService ?? throw new ArgumentNullException(nameof(crmService));
            this.apiConfiguration = GetApiConfiguration();
        }

        public async Task<IndeedJobDetails> GetJobDetails(string id)
        {
            var request = CreateHttpGetRequest(JobDetails.Url + id);
            return await httpRequestService.ExecuteGetRequest<IndeedJobDetails>(request);
        }

        public async Task<IEnumerable<IndeedHit>> GetJobs(Entity integrationSetting)
        {
            //var integrationSettings = crmService.GetIntegrationSettings();
            var numberOfPages = integrationSetting.GetAttributeValue<int>(IntegrationSettings.NumberOfPages);

            var uri = CreateUri(integrationSetting);
            uri += JobSearch.Page + numberOfPages.ToString();

            var request = CreateHttpGetRequest(uri);
            var response = await httpRequestService.ExecuteGetRequest<IndeedResponse>(request);

            return response.Hits;
        }

        public async Task ProcessJob(IBlobStorageService blobStorageService, List<IndeedJobDetails> indeedJobDetails, IndeedHit job)
        {
            var jobDetails = await GetJobDetails(job.Id);
            if (!jobDetails.CreationDate.Equals(IndeedHitConstants.More30Days, StringComparison.InvariantCultureIgnoreCase))
            {
                var indeedBlob = new IndeedBlob
                {
                    Description = jobDetails.Description,
                    Title = jobDetails.Title,
                    Url = jobDetails.FinalUrl
                };

                var exists = blobStorageService.RecordExistsInBlobTable(job.Id);
                if (!exists)
                {
                    jobDetails.JobId = job.Id;
                    indeedJobDetails.Add(jobDetails);
                    var hash = indeedBlob.GetHash();
                    blobStorageService.InsertRecordToTable(job.Id, hash.ToString());
                }
            }
        }

        public OrganizationResponse CreateCrmJobs(IEnumerable<IndeedJobDetails> jobDetails)
        {
            return crmService.BulkCreate(jobDetails);
        }

        public string GetColdLeadExternalId(string jsonContent)
        {
            return crmService.GetColdLeadExternalId(jsonContent);
        }

        private IndeedApiConfiguration GetApiConfiguration()
        {
            return crmService.GetApiConfiguration();
        }

        private HttpRequestMessage CreateHttpGetRequest(string uri)
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
            var query = item.GetAttributeValue<string>(IntegrationSettings.Query).Replace(" ", StringSymbols.Space);
            var location = item.GetAttributeValue<string>(IntegrationSettings.Location);
            var uri = JobSearch.Url + JobSearch.Query + $"{query}" + JobSearch.Location + $"{location}";

            if (((OptionSetValue)item[IntegrationSettings.Localization]).Value != (int)Localization.DontIncludeLocalization)
                uri += JobSearch.Locality + $"{item.FormattedValues[IntegrationSettings.Localization].ToLower()}";

            return uri;
        }
    }
}
