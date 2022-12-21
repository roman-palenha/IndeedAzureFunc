using JobPostingIntegrationFunctions.Models;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services.Interfaces
{
    public interface IGetJobDetailsService
    {
        Task<IndeedJobDetails> SendRequestAsync(string uri, ApiConfiguration apiConfiguration);
    }
}
