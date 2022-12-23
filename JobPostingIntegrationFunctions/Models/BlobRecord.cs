using Microsoft.WindowsAzure.Storage.Table;

namespace JobPostingIntegrationFunctions.Models
{
    public class BlobRecord : TableEntity
    {
        public string Id { get; set; }
        public string Hash { get; set; }
    }
}
