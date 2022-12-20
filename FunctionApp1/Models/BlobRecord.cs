using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
