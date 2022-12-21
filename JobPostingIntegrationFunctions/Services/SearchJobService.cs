using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Helpers;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services
{
    public class SearchJobService : ISearchJobService
    {
        public async Task<IEnumerable<IndeedHit>> SendRequestAsync(string uri, ApiConfiguration apiConfiguration)
        {
            var client = new HttpClient();
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

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var vacancies = IndeedHelper.GetIndeedHitsFromResponse(body);
                return vacancies;
            }
        }
    }
}
