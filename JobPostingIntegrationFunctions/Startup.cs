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
        public static IServiceProvider ConfigureIndeedServices(IOrganizationService organizationService)
        {
            var services = new ServiceCollection()
                .AddSingleton<IIndeedApiConfiguration, IndeedApiConfiguration>()
                .AddScoped<IHttpRequestService, HttpRequestService>();

            services.AddScoped<IIndeedJobService>(s => 
                ActivatorUtilities.CreateInstance<IndeedJobService>(s, new CrmService(organizationService), s.GetService<IHttpRequestService>()));

            return services.BuildServiceProvider();
        }

        public static IServiceProvider ConfigureCRMServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(s =>
                {
                    CrmServiceClient conn = new CrmServiceClient(ConfigurationManager.AppSettings[ConnectionStrings.CRMConnectionString]);
                    return conn.OrganizationWebProxyClient ?? (IOrganizationService)conn.OrganizationServiceProxy;
                });

            return services.BuildServiceProvider();
        }

        public static IServiceProvider ConfigureAzureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton<IAzureBlobConfiguration, AzureBlobConfiguration>()
                .AddScoped<IBlobStorageService, BlobStorageService>();

            return services.BuildServiceProvider();
        }
    }
}
