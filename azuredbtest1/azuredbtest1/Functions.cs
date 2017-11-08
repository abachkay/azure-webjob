using azuredbtest1.Common;
using Microsoft.Azure.WebJobs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace azuredbtest1
{
    public class Functions
    {
        public static async Task ProcessBulkUpdate(
            [QueueTrigger("bulkupdaterequests")] Tuple<BulkUpdateRequestTableEntity, int[]> requestInfo)
        {
            try
            {
                var request = requestInfo.Item1;

                #region TODO: Replace with real logic

                const string golferId = "GolferId";
                var specialUpdateRequests = requestInfo.Item2.Select(clubId => new BulkUpdateRequestTableEntity()
                    {
                        PartitionKey = request.PartitionKey,
                        RowKey = $"{request.PartitionKey}_{clubId}_{golferId}",
                        ClubId = clubId,
                        GolferId = golferId,
                        DateOfRevision = DateTime.UtcNow,
                        Hi9HDisplayValue = "5",
                        Hi18HDisplayValue = "10"
                    })
                    .ToList();
                await Task.Delay(5000);

                #endregion

                await AzureTableAdapter.UpsertMany(specialUpdateRequests, "BulkUpdateRequests");

                request.Status = "Done";
                //throw new Exception("abc");
                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
            }
            catch (Exception exception)
            {
                var request = requestInfo.Item1;
                request.Status = "Error";
                request.Error = exception.Message;
                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
            }
        }

        //public static async Task HandleBulkUpdateError(
        //    [QueueTrigger("bulkupdaterequests-poison")] Tuple<BulkUpdateRequestTableEntity, int[]> requestInfo)
        //{
        //    var request = requestInfo.Item1;
        //    request.Status = "Error";

        //    await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
        //}
    }
}