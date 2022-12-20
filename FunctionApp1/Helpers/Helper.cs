using FunctionApp1.Constants;
using FunctionApp1.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
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

        public static void BulkCreate(IOrganizationService service, IEnumerable<IndeedJobDetails> jobDetails)
        {
            ExecuteMultipleRequest request = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };

            foreach (var d in jobDetails)
            {
                Entity detailEntity = new Entity(EntityName.ColdLeads);
                detailEntity[ColdLead.Name] = d.Title;
                detailEntity[ColdLead.Url] = d.FinalUrl;
                detailEntity[ColdLead.ExternalId] = d.JobId;
                detailEntity[ColdLead.Description] = d.Description;
                detailEntity[ColdLead.CreatedOn] = ParseIndeedCreationDate(d.CreationDate);

                CreateRequest cr = new CreateRequest { Target = detailEntity };
                request.Requests.Add(cr);
            }

           var response =  service.Execute(request);
        }

        private static DateTime ParseIndeedCreationDate(string creationDate)
        {
            var dateTime = DateTime.Now;
            if (creationDate == IndeedHitConstants.JustPosted)
            {
                return dateTime;
            }
            else
            {
                var days = int.Parse(creationDate.Split(' ')[0]);
                dateTime = dateTime.Subtract(TimeSpan.FromDays(days));
            }
        
            return dateTime;
        }

        private static string GetConnectionString()
        {
            var conn = Environment.GetEnvironmentVariable("ConnectionString");
            return conn;
        }
    }
}
