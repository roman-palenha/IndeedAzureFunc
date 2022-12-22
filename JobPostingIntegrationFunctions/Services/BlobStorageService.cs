using JobPostingIntegrationFunctions.Configurations;
using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

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
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var record = GetRecordFromTable(id);

            TableOperation tableOperation = TableOperation.Delete(record);
            table.Execute(tableOperation);
        }

        public BlobRecord GetRecordFromTable(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            TableOperation tableOperation = TableOperation.Retrieve<BlobRecord>(id, id);
            TableResult tableResult = table.Execute(tableOperation);

            return tableResult.Result as BlobRecord;
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
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (hash == null)
                throw new ArgumentNullException(nameof(hash));

            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var record = new BlobRecord()
            {
                Id = id,
                Hash = hash,
            };
            record.AssignRowKey();
            record.AssignPartitionKey();

            TableOperation tableOperation = TableOperation.InsertOrReplace(record);
            table.Execute(tableOperation);
        }

        private CloudTable GetAzureTable(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var storageAccount = CloudStorageAccount.Parse(azureBlobConfiguration.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var cloudTable = tableClient.GetTableReference(name);
            cloudTable.CreateIfNotExists();

            return cloudTable;
        }
    }
}
