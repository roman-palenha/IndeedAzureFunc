using JobPostingIntegrationFunctions.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services.Interfaces
{
    public interface IHttpRequestService
    {
        Task<TResponse> ExecuteGetRequest<TResponse>(IIndeedApiConfiguration apiConfiguration, string uri);
    }
}