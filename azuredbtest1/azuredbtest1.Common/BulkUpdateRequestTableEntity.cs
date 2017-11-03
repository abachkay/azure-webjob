using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace azuredbtest1.Common
{
    public class BulkUpdateRequestTableEntity: TableEntity
    {
        public BulkUpdateRequestTableEntity()
        {
        }
        public BulkUpdateRequestTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
       
        public string Status { get; set; }

        public DateTime DateOfStart { get; set; }

        public string Error { get; set; }
    }
}