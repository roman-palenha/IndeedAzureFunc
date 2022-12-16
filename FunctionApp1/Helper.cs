using FunctionApp1.Models;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text.Json.Nodes;

namespace FunctionApp1
{
    public static class Helper
    {
        public static IOrganizationService Connection(ILogger log)
        {
            IOrganizationService service = null;

            var conn = GetConnectionString();

            var svc = new CrmServiceClient(conn);
            service = svc.OrganizationWebProxyClient != null ? svc.OrganizationWebProxyClient : (IOrganizationService)svc.OrganizationServiceProxy;

            if (service != null)
            {
                Guid userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;

                if (userid != Guid.Empty)
                {
                    log.LogInformation("Connection Established Successfully...");
                }
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
            return indeedResponse.hits;
        }

        private static string GetConnectionString()
        {
            var url = Environment.GetEnvironmentVariable("D365ServiceUrl");
            var username = Environment.GetEnvironmentVariable("Login");
            var password = Environment.GetEnvironmentVariable("Password");

            string conn = $"AuthType=Office365;Username={username}; Password={password};Url={url}";

            return conn;
        }
    }
}
