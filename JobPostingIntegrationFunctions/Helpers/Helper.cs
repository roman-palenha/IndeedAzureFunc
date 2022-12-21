using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;

namespace JobPostingIntegrationFunctions.Helpers
{
    public static class Helper
    {
        public static IOrganizationService Connection(ILogger log)
        {
            var conn = GetConnectionString();

            var svc = new CrmServiceClient(conn);
            log.LogError(svc.LastCrmError);
            IOrganizationService service = svc.OrganizationWebProxyClient != null ? svc.OrganizationWebProxyClient : (IOrganizationService)svc.OrganizationServiceProxy;

            if (service != null)
            {
                log.LogInformation("Succesfully connected to Dynamics 365");
            }
            else
            {
                log.LogError("Failed to connect to Dynamic 365");
            }

            return service;
        }

        private static string GetConnectionString()
        {
            var conn = Environment.GetEnvironmentVariable("CRMConnectionString");
            return conn;
        }
    }
}
