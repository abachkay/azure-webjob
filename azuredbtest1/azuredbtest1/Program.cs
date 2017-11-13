using azuredbtest1.Common;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Usga.Hcs.Common.Enums;

namespace azuredbtest1
{
    internal class Program
    {
        private static void Main()
        {
            //var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            //var queueClient = storageAccount.CreateCloudQueueClient();
            //var queue = queueClient.GetQueueReference("bulkupdaterequests");
            
            //var message = queue.GetMessage();
            //var messageResult = message.AsString;
            ////var clubIds = messageResult.Split(',');

            //var clubIds = AzureTableAdapter.GetByPartKey<BulkUpdateRequestTableEntity>("BulkUpdateRequests", messageResult);

            CheckQueuedRequests().Wait();
        }
        
        public static async Task ProcessBulkUpdate(string partitionKey)
        {
            var request = await
                AzureTableAdapter.GetByRowKeyAndPartKey<BulkUpdateRequestTableEntity>("BulkUpdateRequests", partitionKey, partitionKey);
            try
            {
                request.DateOfStart = DateTime.UtcNow;
                request.Status = BulkUpdateStatus.InProgress;

                await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");

                var results = (await AzureTableAdapter.GetByPartKey<BulkUpdateRequestTableEntity>("BulkUpdateRequests", partitionKey)).ToList().Where(r => r.PartitionKey != r.RowKey);
                #region TODO: Replace with real logic

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
                //await Task.Delay(3000);

                #endregion                
                await AzureTableAdapter.UpsertMany(specialUpdateRequests, "BulkUpdateRequests");

                //throw new Exception("abc");

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
        private static async Task CheckQueuedRequests()
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            var queueClient = storageAccount.CreateCloudQueueClient();            
            while (true)
            {
                var queue = queueClient.GetQueueReference("bulkupdaterequests");
                queue.CreateIfNotExists();
                if (queue.ApproximateMessageCount == 0)
                {
                    break;
                }
                var message = queue.GetMessage();
                var partitionKey = message.AsString;

                await ProcessBulkUpdate(partitionKey);
                queue.DeleteMessage(message);
            }
        }
    }
}