using JobPostingIntegrationFunctions.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services
{
    public class HttpRequestService : IHttpRequestService
    {
        public async Task<TResponse> ExecuteGetRequest<TResponse>(HttpRequestMessage httpRequest)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.SendAsync(httpRequest))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var details = JsonConvert.DeserializeObject<TResponse>(body);
                    return details;
                }
            }
        }
    }
}
