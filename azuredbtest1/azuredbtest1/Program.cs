using Microsoft.Azure.WebJobs;
using System;

namespace azuredbtest1
{
    internal class Program
    {
        private static void Main()
        {
            var config = new JobHostConfiguration();
            config.Queues.MaxDequeueCount = 5;
            config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(2);
            config.Queues.BatchSize = 32;

            var host = new JobHost(config);
            host.RunAndBlock();

            //var guid = SequentialGuidGenerator.NewSequentialId();
            //var request = new BulkUpdateRequestTableEntity()
            //{
            //    PartitionKey = guid.ToString(),
            //    RowKey = guid.ToString(),
            //    Status = "Queued",
            //    DateOfStart = DateTime.UtcNow
            //};

            //Task.WaitAll(AzureTableAdapter.Upsert(request, "BulkUpdateRequests"));

            //Thread.Sleep(10000);
        }
    }
}
