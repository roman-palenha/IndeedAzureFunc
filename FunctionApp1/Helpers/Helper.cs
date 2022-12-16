using FunctionApp1.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FunctionApp1.Helpers
{
    public static class Helper
    {
        public static IOrganizationService Connection(ILogger log)
        {
            var conn = GetConnectionString();

            var svc = new CrmServiceClient(conn);
            IOrganizationService service = svc.OrganizationWebProxyClient != null ? svc.OrganizationWebProxyClient : (IOrganizationService)svc.OrganizationServiceProxy;

            if (service != null)
            {
                log.LogInformation("Connection Established Successfully...");
            }
            else
            {
                log.LogInformation("Failed to Established Connection!!!");
            }
            return service;
        }

        public static string CheckAndReplaceQuery(string query)
        {
            var result = query.Replace(" ", "%20");
            return result;
        }

        public static IEnumerable<IndeedHit> GetIndeedHitsFromResponse(string response)
        {
            var indeedResponse = JsonConvert.DeserializeObject<IndeedResponse>(response);
            return indeedResponse.Hits;
        }

        private static string GetConnectionString()
        {
            var conn = Environment.GetEnvironmentVariable("ConnectionString");
            return conn;
        }
    }
}
