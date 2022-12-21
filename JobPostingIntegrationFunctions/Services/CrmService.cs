using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobPostingIntegrationFunctions.Services
{
    public class CrmService : ICrmService
    {
        private readonly IOrganizationService service;

        public CrmService(IOrganizationService service)
        {
            this.service = service;
        }

        public IndeedApiConfiguration GetApiConfiguration()
        {
            var configurationName = Environment.GetEnvironmentVariable("ApiConfiguration");
            var configurationColumns = new ColumnSet(ConfigurationSettings.Name, ConfigurationSettings.RequestUrl, ConfigurationSettings.RapidHost, ConfigurationSettings.RapidKey);
            var expr = new QueryExpression
            {
                EntityName = EntityName.ConfigurationSettings,
                ColumnSet = configurationColumns,
                TopCount = 1
            };

            var configuration = service.RetrieveMultiple(expr)
                .Entities
                .FirstOrDefault(x => x.Attributes[ConfigurationSettings.Name].ToString().Equals(configurationName));

            var apiConfiguration = new IndeedApiConfiguration
            {
                ApiHost = configuration[ConfigurationSettings.RapidHost].ToString(),
                ApiKey = configuration[ConfigurationSettings.RapidKey].ToString()
            };

            return apiConfiguration;
        }

        public List<Entity> GetIntegrationSettings()
        {
            var integrationColumns = new ColumnSet(IntegrationSettings.Name, IntegrationSettings.JobPortal, IntegrationSettings.Query, IntegrationSettings.Localization, IntegrationSettings.Location, IntegrationSettings.NumberOfPages);
            var expr = new QueryExpression
            {
                EntityName = EntityName.IntegrationSettings,
                ColumnSet = integrationColumns,
                //TODO
                //Criteria = where JobsPortal == Indeed 
            };

            var integrationSettings = service.RetrieveMultiple(expr)
                .Entities.ToList();

            return integrationSettings;
        }

        public OrganizationResponse BulkCreate(IEnumerable<IndeedJobDetails> jobDetails)
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

            var response = service.Execute(request);
            return response;
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
    }
}
