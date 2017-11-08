using azuredbtest1.Common;
using Microsoft.Azure.WebJobs;
using System;
using System.Linq;
using System.Threading.Tasks;
using Usga.Hcs.DataAccess.DataAccess;

namespace azuredbtest1
{
    public class Functions
    {
        [NoAutomaticTrigger]
        public static async Task ProcessBulkUpdate(int[] clubIds)
        {
            var request = new BulkUpdateRequestTableEntity();
            try
            {
                var guid = SequentialGuidGenerator.NewSequentialId();
                request = new BulkUpdateRequestTableEntity
                {
                    PartitionKey = guid.ToString(),
                    RowKey = guid.ToString(),
                    Status = "Inprogress",
                    DateOfStart = DateTime.UtcNow
                };

                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");

                #region TODO: Replace with real logic

                const string golferId = "GolferId";
                var specialUpdateRequests = clubIds.Select(clubId => new BulkUpdateRequestTableEntity
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
                request.Status = "Error";
                request.Error = exception.Message;
                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
            }
        }
    }
}