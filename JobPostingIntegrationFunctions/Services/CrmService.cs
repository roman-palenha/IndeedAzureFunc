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
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IndeedApiConfiguration GetApiConfiguration()
        {
            var configurationName = Environment.GetEnvironmentVariable(AppConfigurations.ApiConfiguration);
            var configurationColumns = new ColumnSet(ConfigurationSettings.Name, ConfigurationSettings.RequestUrl, ConfigurationSettings.RapidHost, ConfigurationSettings.RapidKey);
            QueryExpression expr = GetApiConfigurationQueryExpression(configurationName, configurationColumns);

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

        public List<IntegrationSettingsDto> GetIntegrationSettings()
        {
            var integrationColumns = new ColumnSet(IntegrationSettings.Name, IntegrationSettings.JobPortal, IntegrationSettings.Query, IntegrationSettings.Localization, IntegrationSettings.Location, IntegrationSettings.NumberOfPages);
            QueryExpression expr = GetIntegrationSettingsQueryExpression(integrationColumns);

            var integrationSettings = service
                .RetrieveMultiple(expr)
                .Entities
                .SelectMany(x => ParseToIntegrationSettingsDto(x))
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

            foreach (var indeedJob in jobDetails)
            {
                Entity detailEntity = new Entity(EntityName.ColdLeads);
                detailEntity[ColdLead.Name] = indeedJob.Title;
                detailEntity[ColdLead.Url] = indeedJob.FinalUrl;
                detailEntity[ColdLead.ExternalId] = indeedJob.JobId;
                detailEntity[ColdLead.Description] = indeedJob.Description;
                detailEntity[ColdLead.CreatedOn] = ParseIndeedCreationDate(indeedJob.CreationDate);

                CreateRequest cr = new CreateRequest { Target = detailEntity };
                request.Requests.Add(cr);
            }

            var response = service.Execute(request);
            return response;
        }

        public string GetColdLeadExternalId(string id)
        {
            var coldLeadsColumn = new ColumnSet(ColdLead.ExternalId);
            var deleteEntity = service.Retrieve(EntityName.ColdLeads, new Guid(id), coldLeadsColumn);
            if (deleteEntity == null)
                throw new NullReferenceException($"Entity with id {id} is not found.");

            var deleteId = deleteEntity.GetAttributeValue<string>(ColdLead.ExternalId);
            return deleteId;
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

        private List<IntegrationSettingsDto> ParseToIntegrationSettingsDto(Entity integrationSettingsEntity)
        {
            var name = integrationSettingsEntity.GetAttributeValue<string>(IntegrationSettings.Name);
            var query = integrationSettingsEntity.GetAttributeValue<string>(IntegrationSettings.Query).Replace(" ", StringSymbols.Space);
            var location = integrationSettingsEntity.GetAttributeValue<string>(IntegrationSettings.Location);
            var numberOfPages = integrationSettingsEntity.GetAttributeValue<int>(IntegrationSettings.NumberOfPages);
            var result = new List<IntegrationSettingsDto>();

            for(int i = 1; i <= numberOfPages; ++i)
            {
                var integrationSettings = new IntegrationSettingsDto
                {
                    Name = name,
                    JobPortal = (JobPortal)((OptionSetValue)integrationSettingsEntity[IntegrationSettings.JobPortal]).Value,
                    Query = query,
                    Localization = (Localization)((OptionSetValue)integrationSettingsEntity[IntegrationSettings.Localization]).Value,
                    Location = location,
                    Page = i
                };

                result.Add(integrationSettings);
            }

            return result;
        }

        private static QueryExpression GetApiConfigurationQueryExpression(string configurationName, ColumnSet configurationColumns)
        {
            return new QueryExpression
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
        }

        private static QueryExpression GetIntegrationSettingsQueryExpression(ColumnSet integrationColumns)
        {
            return new QueryExpression
            {
                EntityName = EntityName.IntegrationSettings,
                ColumnSet = integrationColumns,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression()
                        {
                            AttributeName = IntegrationSettings.JobPortal,
                            Operator = ConditionOperator.Equal,
                            Values =
                            {
                                (int)JobPortal.Indeed
                            }
                        }
                    }
                }
            };
        }
    }
}
