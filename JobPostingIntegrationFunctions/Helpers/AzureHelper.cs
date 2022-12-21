using JobPostingIntegrationFunctions.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace JobPostingIntegrationFunctions.Helpers
{
    public static class AzureHelper
    {
        public static CloudQueue GetAzureQueue(string name)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureConnectionString");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(name);
            queue.CreateIfNotExists();

            return queue;
        }

        public static CloudTable GetAzureTable(string name)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureConnectionString");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var cloudTable = tableClient.GetTableReference(name);
            cloudTable.CreateIfNotExists();

            return cloudTable;
        }

        public static void InsertRecordToTable(string id, string hash)
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

        public static BlobRecord GetRecordFromTable(string id)
        {
            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            TableOperation tableOperation = TableOperation.Retrieve<BlobRecord>(id, id);
            TableResult tableResult = table.Execute(tableOperation);

            return tableResult.Result as BlobRecord;
        }

        public static IEnumerable<BlobRecord> GetRecordsFromTable()
        {
            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var query = new TableQuery<BlobRecord>();
            var tableResult = table.ExecuteQuery(query);

            return tableResult;
        }

        public static void DeleteRecordFromTable(string id)
        {
            var table = GetAzureTable(Constants.AzureTable.IndeedJobs);
            var record = GetRecordFromTable(id);
            TableOperation tableOperation = TableOperation.Delete(record);
            TableResult tableResult = table.Execute(tableOperation);
        }
    }
}
