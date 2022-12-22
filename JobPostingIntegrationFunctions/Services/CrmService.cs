using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
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
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IndeedApiConfiguration GetApiConfiguration()
        {
            var configurationName = Environment.GetEnvironmentVariable(AppConfigurations.ApiConfiguration);
            var configurationColumns = new ColumnSet(ConfigurationSettings.Name, ConfigurationSettings.RequestUrl, ConfigurationSettings.RapidHost, ConfigurationSettings.RapidKey);
            var expr = new QueryExpression
            {
                EntityName = EntityName.ConfigurationSettings,
                ColumnSet = configurationColumns,
                TopCount = 1,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression()
                        {
                            AttributeName = ConfigurationSettings.Name,
                            Operator = ConditionOperator.Equal,
                            Values =
                            {
                                configurationName
                            }
                        }
                    }
                }
            };

            var configuration = service.RetrieveMultiple(expr)
                .Entities
                .FirstOrDefault();

            if (configuration == null || configuration[ConfigurationSettings.RapidHost] == null || configuration[ConfigurationSettings.RapidKey] == null)
                throw new NullReferenceException("Configuration is null.");

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
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression() { 
                            AttributeName = IntegrationSettings.JobPortal, 
                            Operator = ConditionOperator.Equal, 
                            Values = {
                                (int)JobPortal.Indeed 
                            } 
                        }
                    }
                }
            };

            var integrationSettings = service
                .RetrieveMultiple(expr)
                .Entities
                .ToList();

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

        public string GetColdLeadExternalId(string jsonContent)
        {
            if (jsonContent == null)
                throw new ArgumentNullException(nameof(jsonContent));

            var entityId = DeserializeCrmRequestBody(jsonContent);
            var coldLeadsColumn = new ColumnSet(ColdLead.ExternalId);
            var deleteEntity = service.Retrieve(EntityName.ColdLeads, new Guid(entityId), coldLeadsColumn);
            if (deleteEntity == null)
                throw new NullReferenceException($"Entity with id {entityId} is not found.");

            var deleteId = deleteEntity[ColdLead.ExternalId].ToString();
            return deleteId;
        }

        private static string DeserializeCrmRequestBody(string jsonContent)
        {
            var content = JsonConvert.DeserializeObject<CrmRequestBody>(jsonContent);
            var entityId = content.PrimaryEntityId;

            return entityId;
        }
        private static DateTime ParseIndeedCreationDate(string creationDate)
        {
            var dateTime = DateTime.Now;
            if (creationDate != IndeedHitConstants.JustPosted && creationDate != IndeedHitConstants.Today)
            {
                var days = int.Parse(creationDate.Split(' ')[0]);
                dateTime = dateTime.Subtract(TimeSpan.FromDays(days));
            }

            return dateTime;
        }
    }
}
