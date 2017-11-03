using System;
using System.Threading.Tasks;
using azuredbtest1.Common;
using Usga.Hcs.DataAccess.DataAccess;

namespace azuredbtest1
{
    internal class Program
    {
        static void Main()
        {
            var guid = SequentialGuidGenerator.NewSequentialId();
            var request = new BulkUpdateRequestTableEntity()
            {
                PartitionKey = guid.ToString(),
                RowKey = guid.ToString(),
                Status = "Queued",
                DateOfStart = DateTime.UtcNow
            };

            Task.WaitAll(AzureTableAdapter.Upsert(request, "BulkUpdateRequests"));
        }
    }
}
