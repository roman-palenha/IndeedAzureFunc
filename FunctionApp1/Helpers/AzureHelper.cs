using FunctionApp1.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FunctionApp1.Helpers
{
    public static class AzureHelper
    {
        public static CloudQueue GetAzureQueue(string name)
        {
            //var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
            var connectionString = Environment.GetEnvironmentVariable("AzureConnectionString");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(name);
            queue.CreateIfNotExists();

            return queue;
        }

        public static CloudTable GetAzureTable(string name)
        {
            //var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
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
    }
}
