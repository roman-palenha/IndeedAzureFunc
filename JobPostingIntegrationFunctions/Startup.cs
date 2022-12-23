using JobPostingIntegrationFunctions.Configurations;
using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;

namespace JobPostingIntegrationFunctions
{
    public static class Startup
    {
        public static IServiceProvider ConfigureIndeedServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(s =>
                {
                    CrmServiceClient conn = new CrmServiceClient(ConfigurationManager.AppSettings[ConnectionStrings.CRMConnectionString]);
                    return conn.OrganizationWebProxyClient ?? (IOrganizationService)conn.OrganizationServiceProxy;
                })
                .AddSingleton<IIndeedApiConfiguration, IndeedApiConfiguration>()
                .AddScoped<IHttpRequestService, HttpRequestService>()
                .AddScoped<ICrmService, CrmService>()
                .AddScoped<IIndeedJobService, IndeedJobService>()
                .AddSingleton<IAzureBlobConfiguration, AzureBlobConfiguration>()
                .AddScoped<IBlobStorageService, BlobStorageService>();

            return services.BuildServiceProvider();
        }
    }
}
