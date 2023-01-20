using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services
{
    public class IndeedJobService : IIndeedJobService
    {
        private readonly ICrmService crmService;
        private readonly IHttpRequestService httpRequestService;
        private readonly IIndeedApiConfiguration apiConfiguration;
        private readonly IBlobStorageService blobStorageService;

        public IndeedJobService(ICrmService crmService, IHttpRequestService httpRequestService, IBlobStorageService blobStorageService)
        {
            this.crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
            this.httpRequestService = httpRequestService ?? throw new ArgumentNullException(nameof(crmService));
            this.blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            this.apiConfiguration = GetApiConfiguration();       
        }

        public async Task GetJobsFromApi(List<IndeedJobDetails> indeedJobDetails)
        {
            var processJobsTasks = new List<Task>();
            var integrationSettings = crmService.GetIntegrationSettings();
            var tasks = integrationSettings
                .Select(x => GetJobs(x))
                .Select(async y => ProcessJobs(await y, indeedJobDetails));

            var res = await Task.WhenAll(tasks);
            await Task.WhenAll(res);
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

        private async Task<IEnumerable<IndeedHit>> GetJobs(IntegrationSettingsDto integrationSetting)
        {
            var uri = CreateUri(integrationSetting);
            uri += JobSearch.Page + integrationSetting.Page.ToString();

            var request = CreateHttpGetRequest(uri);
            var response = await httpRequestService.ExecuteGetRequest<IndeedResponse>(apiConfiguration, uri);

            return response.Hits;
        }

        private async Task<IndeedJobDetails> GetJobDetails(string id)
        {
            var request = CreateHttpGetRequest(JobDetails.Url + id);
            return await httpRequestService.ExecuteGetRequest<IndeedJobDetails>(apiConfiguration, JobDetails.Url + id);
        }

        private async Task ProcessJobs(IEnumerable<IndeedHit> jobs, List<IndeedJobDetails> indeedJobDetails)
        {
            var processJobTasks = new List<Task>();
            foreach (var job in jobs)
            {
                processJobTasks.Add(ProcessJob(blobStorageService, indeedJobDetails, job));
            }

            await Task.WhenAll(processJobTasks);
        }

        private async Task ProcessJob(IBlobStorageService blobStorageService, List<IndeedJobDetails> indeedJobDetails, IndeedHit job)
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

        private string CreateUri(IntegrationSettingsDto integrationSettings)
        {
            var query = integrationSettings.Query;
            var location = integrationSettings.Location;

            var uri = JobSearch.Url + JobSearch.Query + $"{query}" + JobSearch.Location + $"{location}";

            if (integrationSettings.Localization != Localization.DontIncludeLocalization)
                uri += JobSearch.Locality + $"{GetLocalization(integrationSettings)}";

            return uri;
        }

        private string GetLocalization(IntegrationSettingsDto integrationSettings)
        {
            if (integrationSettings.Localization == Localization.CHFR)
                return LocalizationStrings.ChFr;

            return integrationSettings.Localization.ToString().ToLower();
        }
    }
}
