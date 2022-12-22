using JobPostingIntegrationFunctions.Models;
using JobPostingIntegrationFunctions.Services.Interfaces;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPostingIntegrationFunctions.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string connectionString;

        public BlobStorageService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void DeleteRecordFromTable(string id)
        {
            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var record = GetRecordFromTable(id);
            TableOperation tableOperation = TableOperation.Delete(record);
            TableResult tableResult = table.Execute(tableOperation);
        }

        public BlobRecord GetRecordFromTable(string id)
        {
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
            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var record = new BlobRecord()
            {
                Id = id,
                Hash = hash,
            };
            record.AssignRowKey();
            record.AssignPartitionKey();

            TableOperation tableOperation = TableOperation.InsertOrReplace(record);
            TableResult tableResult = table.Execute(tableOperation);
        }

        private CloudQueue GetAzureQueue(string name)
        {       
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(name);
            queue.CreateIfNotExists();

            return queue;
        }

        private CloudTable GetAzureTable(string name)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var cloudTable = tableClient.GetTableReference(name);
            cloudTable.CreateIfNotExists();

            return cloudTable;
        }
    }
}
