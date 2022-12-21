using JobPostingIntegrationFunctions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services.Interfaces
{
    public interface ISearchJobService
    {
        Task<IEnumerable<IndeedHit>> SendRequestAsync(string uri, ApiConfiguration apiConfiguration);
    }
}
