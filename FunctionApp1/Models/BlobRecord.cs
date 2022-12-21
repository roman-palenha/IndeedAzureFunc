using Microsoft.WindowsAzure.Storage.Table;

namespace FunctionApp1.Models
{
    public class BlobRecord : TableEntity
    {
        public string Id { get; set; }
        public string Hash { get; set; }

        public void AssignRowKey()
        {
            this.RowKey = Id.ToString();
        }

        public void AssignPartitionKey()
        {
            this.PartitionKey = Id.ToString();
        }
    }
}
