using JobPostingIntegrationFunctions.Models;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services.Interfaces
{
    public interface IIndeedJobService
    {
        Task GetJobsFromApi(IBlobStorageService blobStorageService, List<IndeedJobDetails> indeedJobDetails);
        OrganizationResponse CreateCrmJobs(IEnumerable<IndeedJobDetails> jobDetails);
        string GetColdLeadExternalId(string jsonContent);
    }
}
