using JobPostingIntegrationFunctions.Constants;
using System;
using System.Configuration;

namespace JobPostingIntegrationFunctions.Configurations
{
    public class AzureBlobConfiguration : IAzureBlobConfiguration
    {
        public string ConnectionString
        {
            get
            {
                var connectionString = ConfigurationManager.AppSettings[ConnectionStrings.AzureConnectionString];

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentNullException(nameof(connectionString));
                }
                return connectionString;
            }
        }
    }
}
