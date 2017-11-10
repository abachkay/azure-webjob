using azuredbtest1.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace azuredbtest1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args == null || !args.Any())
            {
                return;
            }
            
            var requestId = args[0];

            ProcessBulkUpdate(requestId).Wait();            
            CheckQueuedRequests().Wait();
        }
        
        public static async Task ProcessBulkUpdate(string requestId)
        {
            var request = new BulkUpdateRequestTableEntity();
            try
            {
                request = await AzureTableAdapter.GetByRowKeyAndPartKey<BulkUpdateRequestTableEntity>("BulkUpdateRequests", requestId, requestId);

                request.DateOfStart = DateTime.UtcNow;
                request.Status = "Inprogress";

                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");

                var clubIds = request.ClubIds.Split(' ');

                #region TODO: Replace with real logic

                const string golferId = "GolferId";
                var specialUpdateRequests = clubIds.Select(clubId => new BulkUpdateRequestTableEntity
                    {
                        PartitionKey = request.PartitionKey,
                        RowKey = $"{request.PartitionKey}_{clubId}_{golferId}",
                        ClubId = Convert.ToInt32(clubId),
                        GolferId = golferId,
                        DateOfRevision = DateTime.UtcNow,
                        Hi9HDisplayValue = "5",
                        Hi18HDisplayValue = "10",
                        Status = "Done"

                    })
                    .ToList();
                await Task.Delay(3000);

                #endregion                
                await AzureTableAdapter.UpsertMany(specialUpdateRequests, "BulkUpdateRequests");

                //throw new Exception("abc");

                request.Status = "Done";
                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
            }
            catch (Exception exception)
            {
                request.Status = "Error";
                request.Error = exception.Message;
                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");
            }
        }
        private static async Task CheckQueuedRequests()
        {
            while (true)
            {
                var requests = await AzureTableAdapter.Query<BulkUpdateRequestTableEntity>("BulkUpdateRequests", "(Status eq 'Queued')");

                if (requests == null || !requests.Any())
                {
                    break;
                }

                foreach (var request in requests)
                {
                    await ProcessBulkUpdate(request.PartitionKey);
                }
            }
        }
    }
}