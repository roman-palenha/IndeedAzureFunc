using JobPostingIntegrationFunctions.Models;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services.Interfaces
{
    public interface IIndeedJobService
    {
        Task<IndeedJobDetails> GetJobDetails(string id);
        Task<IEnumerable<IndeedHit>> GetJobs();
        OrganizationResponse CreateCrmJobs(IEnumerable<IndeedJobDetails> jobDetails);
        string GetColdLeadExternalId(string jsonContent);
        Task ProcessJob(IBlobStorageService blobStorageService, List<IndeedJobDetails> indeedJobDetails, IndeedHit job);
    }
}
