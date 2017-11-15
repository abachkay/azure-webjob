﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using azuredbtest1.Common;


namespace Usga.Hcs.Common.Model
{
    public class BulkUpdateRequestTableEntity : TableEntity
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

        public DateTime? DateOfStart { get; set; }

        public string Error { get; set; }

        public string ClubIdsString { get; set; }      
    }
}