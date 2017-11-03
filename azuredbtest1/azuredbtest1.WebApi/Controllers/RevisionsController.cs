using System;
using azuredbtest1.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.Azure.WebJobs;
using Usga.Hcs.DataAccess.DataAccess;

namespace azuredbtest1.WebApi.Controllers
{
    [RoutePrefix("api/bulkupdate")]
    public class RevisionsController : ApiController
    {
        private const string TableName = "BulkUpdateRequests";

        [Route("")]
        [HttpGet]
        public IHttpActionResult GetAll()
        {
            var results = AzureTableAdapter.GetAll<BulkUpdateRequestTableEntity>(TableName);

            return new OkNegotiatedContentResult<IEnumerable<BulkUpdateRequestTableEntity>>(results, this);
        }

        [Route("{requestId}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(string requestId)
        {
            var result = await 
                AzureTableAdapter.GetByRowKeyAndPartKey<BulkUpdateRequestTableEntity>(TableName, requestId, requestId);

            return new OkNegotiatedContentResult<BulkUpdateRequestTableEntity>(result, this);
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody]int[] clubIds)
        {
            var guid = SequentialGuidGenerator.NewSequentialId();
            var request = new BulkUpdateRequestTableEntity()
            {
                PartitionKey = guid.ToString(),
                RowKey = guid.ToString(),
                Status = "Queued",
                DateOfStart = DateTime.UtcNow
            };

            await AzureTableAdapter.Upsert(request, TableName);

            //var config = new JobHostConfiguration();
            //config.UseTimers();
            //config.Queues.MaxDequeueCount = 2;
            //config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(4);
            //config.Queues.BatchSize = 2;
            //var host = new JobHost(config);
            //host.RunAndBlock();

            return new OkNegotiatedContentResult<BulkUpdateRequestTableEntity>(request, this);
        }

        [Route("{requestId}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(string requestId)
        {
            await AzureTableAdapter.Delete<BulkUpdateRequestTableEntity>(requestId, requestId, TableName);
            return Ok();
        }
    }
}
