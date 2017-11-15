using azuredbtest1.Common;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usga.Hcs.Common.Model;

namespace azuredbtest1
{
    public class Functions
    {
        public static async Task ProcessBulkUpdate(
            [QueueTrigger("bulkupdate")] Guid jobId)
        {            
            var request = await
                AzureTableAdapter.GetByRowKeyAndPartKey<BulkUpdateRequestTableEntity>("bulkupdate",
                    jobId.ToString(), jobId.ToString());
            request.DateOfStart = DateTime.UtcNow;
            request.Status = BulkUpdateStatus.InProgress;

            await AzureTableAdapter.Upsert(request, "bulkupdate");

            var rand = new Random();
            var requestData = new List<BulkUpdateDataTableEntity>();

            for (var i = 0; i < 3; i++)
            {
                var golferId = $"a{rand.Next(1, 10000)}";
                var data = new BulkUpdateDataTableEntity
                {
                    PartitionKey = jobId.ToString(),
                    RowKey = jobId.ToString()+golferId+112,
                    GolferId = golferId,
                    ClubId = 112,
                    DateOfRevision = DateTime.UtcNow,
                    Hi9HDisplayValue = "5",
                    Hi18HDisplayValue = "10",
                    Status = BulkUpdateStatus.Done
                };
                requestData.Add(data);
            }

            await AzureTableAdapter.UpsertMany(requestData, "bulkupdate");

            request.Status = BulkUpdateStatus.Done;
            await AzureTableAdapter.Upsert(request, "bulkupdate");
        }
    }
}