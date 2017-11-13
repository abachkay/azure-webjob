using System;
using Microsoft.WindowsAzure.Storage.Table;
using Usga.Hcs.Common.Enums;

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

        public BulkUpdateStatus Status
        {
            get => (BulkUpdateStatus)StatusIntegerValue;
            set => StatusIntegerValue = (int)value;
        }

        public int StatusIntegerValue { get; set; }
        
        public DateTime? DateOfStart { get; set; }

        public string Error { get; set; }

        public string GolferId { get; set; }

        public int? ClubId { get; set; }

        public DateTime? DateOfRevision { get; set; }

        public string Hi9HDisplayValue { get; set; }

        public string Hi18HDisplayValue { get; set; }
    }
}