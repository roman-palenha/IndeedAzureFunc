using JobPostingIntegrationFunctions.Services;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JobPostingIntegrationFunctions
{
    public static class Startup
    {
        public static IServiceProvider ConfigureIndeedServices()
        {
            var services = new ServiceCollection()
                .AddSingleton<ISearchJobService, SearchJobService>()
                .AddSingleton<IGetJobDetailsService, GetJobDetailsService>();

            return services.BuildServiceProvider();
        }
    }
}
