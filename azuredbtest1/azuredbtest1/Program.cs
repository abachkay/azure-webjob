using Microsoft.Azure.WebJobs;
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
                args = new [] {"1"};
            }

            var config = new JobHostConfiguration();

            var clubIds = args.Select(a => Convert.ToInt32(a)).ToArray();
            var host = new JobHost(config);
            Task.WaitAll( host.CallAsync(typeof(Functions).GetMethod("ProcessBulkUpdate"), new { clubIds }));

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
