using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Configurations
{
    public class AzureBlobConfiguration : IAzureBlobConfiguration
    {
        public string ConnectionString { get; set; }
    }
}
