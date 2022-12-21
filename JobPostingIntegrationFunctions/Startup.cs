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
                .AddSingleton<IIndeedApiConfiguration, IndeedApiConfiguration>()
                .AddScoped<IIndeedJobService, IndeedJobService>();

            return services.BuildServiceProvider();
        }

        public static IServiceProvider ConfigureCRMServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(s =>
                {
                    CrmServiceClient conn = new CrmServiceClient(ConfigurationManager.AppSettings["CRMConnectionString"]);
                    return (IOrganizationService)conn.OrganizationServiceProxy;
                });

            return services.BuildServiceProvider();
        }

        public static IServiceProvider ConfigureIndeedServices(IOrganizationService organizationService)
        {
            var services = new ServiceCollection()
                .AddScoped(s =>
                {
                    return new CrmService(organizationService);
                });

            return services.BuildServiceProvider();
        }
    }
}
