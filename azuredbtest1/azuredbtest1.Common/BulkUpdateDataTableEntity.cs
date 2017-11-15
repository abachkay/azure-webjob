using Microsoft.WindowsAzure.Storage.Table;
using System;
using azuredbtest1.Common;


namespace Usga.Hcs.Common.Model
{
    public class BulkUpdateDataTableEntity : TableEntity
    {
        public BulkUpdateDataTableEntity()
        {
        }
        public BulkUpdateDataTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public BulkUpdateStatus Status
        {
            get
            {
                return StatusIntegerValue == null ? BulkUpdateStatus.Error : (BulkUpdateStatus)StatusIntegerValue;
            }
            set
            {
                StatusIntegerValue = (int)value;
            }
        }

        public Guid RequestId { get; set; }

        public int? StatusIntegerValue { get; set; }

        public string Error { get; set; }       

        public string GolferId { get; set; }

        public int? ClubId { get; set; }

        public DateTime? DateOfRevision { get; set; }

        public string Hi9HDisplayValue { get; set; }

        public string Hi18HDisplayValue { get; set; }
    }
}