using JobPostingIntegrationFunctions.Configurations;
using JobPostingIntegrationFunctions.Constants;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobPostingIntegrationFunctions.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IAzureBlobConfiguration azureBlobConfiguration;

        public BlobStorageService(IAzureBlobConfiguration azureBlobConfiguration)
        {
            this.azureBlobConfiguration = azureBlobConfiguration ?? throw new ArgumentNullException(nameof(azureBlobConfiguration));
        }

        public void DeleteRecordFromTable(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var record = GetRecordFromTable(id);

            TableOperation tableOperation = TableOperation.Delete(record);
            table.Execute(tableOperation);
        }

        public BlobRecord GetRecordFromTable(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var tableQuery = new TableQuery<BlobRecord>().Where(TableQuery.GenerateFilterCondition(AzureTable.IdColumnName, QueryComparisons.Equal, id)).Take(1);
            var tableResult = table.ExecuteQuery(tableQuery).FirstOrDefault();

            return tableResult;
        }

        public IEnumerable<BlobRecord> GetRecordsFromTable()
        {
            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var query = new TableQuery<BlobRecord>();
            var tableResult = table.ExecuteQuery(query);

            return tableResult;
        }

        public void InsertRecordToTable(string id, string hash)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (string.IsNullOrEmpty(hash))
                throw new ArgumentNullException(nameof(hash));

            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var record = new BlobRecord()
            {
                Id = id,
                Hash = hash,
                RowKey = id.ToString(),
                PartitionKey = Guid.NewGuid().ToString()
            };

            TableOperation tableOperation = TableOperation.InsertOrReplace(record);
            table.Execute(tableOperation);
        }

        public bool RecordExistsInBlobTable(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var record = GetRecordFromTable(id);

            return record != null;
        }

        private CloudTable GetAzureTable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var storageAccount = CloudStorageAccount.Parse(azureBlobConfiguration.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var cloudTable = tableClient.GetTableReference(name);
            cloudTable.CreateIfNotExists();

            return cloudTable;
        }
    }
}
