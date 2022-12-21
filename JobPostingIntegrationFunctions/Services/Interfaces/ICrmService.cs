using JobPostingIntegrationFunctions.Models;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace JobPostingIntegrationFunctions.Services.Interfaces
{
    public interface ICrmService
    {
        OrganizationResponse BulkCreate(IEnumerable<IndeedJobDetails> jobDetails);
        IndeedApiConfiguration GetApiConfiguration();
        List<Entity> GetIntegrationSettings();
    }
}