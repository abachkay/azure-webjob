using azuredbtest1.Common;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading.Tasks;

namespace azuredbtest1
{
    public class Functions
    {
        public static async Task ProcessBulkUpdate(
            [QueueTrigger("bulkupdaterequests")] Guid jobId)
        {
            var request = await
                AzureTableAdapter.GetByRowKeyAndPartKey<BulkUpdateRequestTableEntity>("BulkUpdateRequests", jobId.ToString(), jobId.ToString());
            try
            {
                request.DateOfStart = DateTime.UtcNow;
                request.Status = BulkUpdateStatus.InProgress;

                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");

                var rand = new Random();
                var requestData = new List<BulkUpdateRequestTableEntity>();

                for (var i = 0; i < 5; i++)
                {
                    var golferId = $"a{rand.Next(1, 10000)}";
                    var data = new BulkUpdateData
                    {
                        GolferId = golferId,
                        ClubId = clubId
                    };

                    requestData.Add(data);
                }

                request.Data = requestData;

                var specialUpdateRequests = results.Select(r => new BulkUpdateRequestTableEntity
                {
                    PartitionKey = r.PartitionKey,
                    RowKey = r.RowKey,
                    ClubId = r.ClubId,
                    GolferId = r.GolferId,
                    DateOfRevision = DateTime.UtcNow,
                    Hi9HDisplayValue = "5",
                    Hi18HDisplayValue = "10",
                    Status = BulkUpdateStatus.Done
                }).ToList();
                await Task.Delay(3000);


                await AzureTableAdapter.UpsertMany(specialUpdateRequests, "BulkUpdateRequests");

                request.Status = BulkUpdateStatus.Done;
                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
            }
            catch (Exception exception)
            {
                request.Status = BulkUpdateStatus.Error;
                request.Error = exception.Message;
                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
            }          
        }      
    }
}