using azuredbtest1.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Usga.Hcs.DataAccess.DataAccess;

namespace azuredbtest1.WebApi.Controllers
{
    [RoutePrefix("api/bulkupdate")]
    public class RevisionsController : ApiController
    {
        private const string TableName = "BulkUpdateRequests";      

        [Route("{requestId}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(string requestId)
        {
            var result = await 
                AzureTableAdapter.GetByPartKey<BulkUpdateRequestTableEntity>(TableName, requestId);

            if (result == null || !result.Any())
            {
                return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.NotFound){Content = new StringContent("Request not found")});
            }

            return new OkNegotiatedContentResult<IEnumerable<BulkUpdateRequestTableEntity>>(result, this);
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] int[] clubIds)
        {
            var guid = SequentialGuidGenerator.NewSequentialId();
            var request = new BulkUpdateRequestTableEntity
            {
                PartitionKey = guid.ToString(),
                RowKey = guid.ToString(),
                Status = BulkUpdateStatus.Queued,
                DateOfStart = DateTime.UtcNow
            };

            await AzureTableAdapter.Upsert(request, "BulkUpdateRequests");

            var message = new CloudQueueMessage(guid.ToString());
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("bulkupdaterequests");

            queue.CreateIfNotExists();
            await queue.AddMessageAsync(message);

            return new OkNegotiatedContentResult<BulkUpdateRequestTableEntity>(request, this);

            //var client = new HttpClient()
            //{
            //    BaseAddress = new Uri("https://azuredbtest2.scm.azurewebsites.net/api/")
            //};
            //var byteArray =
            //    Encoding.ASCII.GetBytes("$azuredbtest2:Sz4wgrHJd8ePpLxXSQqbuWHyEDqqMjvrEbGnul9XYmtQkvgCymFg5JQwQRk4");
            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            //var response = await client.PostAsync("triggeredwebjobs/azuredbtest1/run", null);
            //return new ResponseMessageResult(response);
        }

        [Route("{requestId}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(string requestId)
        {
            var result = await
                AzureTableAdapter.GetByPartKey<BulkUpdateRequestTableEntity>(TableName, requestId);

            if (result == null || !result.Any())
            {
                return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Request not found") });
            }

            await Task.WhenAll(result.Select(r =>
                AzureTableAdapter.Delete<BulkUpdateRequestTableEntity>(r.RowKey, r.PartitionKey, TableName))); 

            return Ok();
        }
    }
}
