using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services
{
    public class HttpRequestService : IHttpRequestService
    {
        private static HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) };
        private int retryCount = 3;
        private readonly TimeSpan delay = TimeSpan.FromSeconds(15);

        public async Task<TResponse> ExecuteGetRequest<TResponse>(IIndeedApiConfiguration apiConfiguration, string uri)
        {
            int currentRetry = 0;

            for (; ; )
            {
                try
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(uri),
                        Headers =
                        {
                        { RequestHeaders.RapidKey, apiConfiguration.ApiKey },
                        { RequestHeaders.RapidHost, apiConfiguration.ApiHost },
                        }
                    };

                    var res = await GetRequest<TResponse>(request);
                    return res;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Operation Exception");

                    currentRetry++;

                    if (currentRetry > this.retryCount || !IsTransient(ex))
                    {
                        throw;
                    }
                }

                await Task.Delay(delay);
            }
        }


        private bool IsTransient(Exception ex)
        {
            if (ex is HttpRequestException)
                return true;

            var webException = ex as WebException;
            if (webException != null)
            {
                return new[] {WebExceptionStatus.ConnectionClosed,
                  WebExceptionStatus.Timeout,
                  WebExceptionStatus.RequestCanceled }.
                        Contains(webException.Status);
            }

            return false;
        }

        private async Task<TResponse> GetRequest<TResponse>(HttpRequestMessage httpRequest)
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
