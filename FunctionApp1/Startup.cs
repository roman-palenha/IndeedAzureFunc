using FunctionApp1.Services;
using FunctionApp1.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class Startup
    {
        public static IServiceProvider ConfigureIndeedServices()
        {
            var services = new ServiceCollection()
                .AddSingleton<ISearchJobService, SearchJobService>();

            return services.BuildServiceProvider();
        }
    }
}
