using Microsoft.Azure.WebJobs.Host;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;

namespace IndeedAzure
{
    public static class Helper
    {
        public static IOrganizationService Connection(ILogger log)
        {
            IOrganizationService service = null;

            #region Credentials Code
            string URL = "orgfd88e666.crm19.dynamics.com";
            string userName = "mykhailo.vashchuk@logiqapps.com";
            string password = "Qaf30514!";
            #endregion

            string conn = $@"Url={URL};AuthType=OAuth;Username={userName};Password ={password};";

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
    }
}
