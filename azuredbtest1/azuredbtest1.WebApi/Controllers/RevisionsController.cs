using azuredbtest1.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Usga.Hcs.DataAccess.DataAccess;

namespace azuredbtest1.WebApi.Controllers
{
    [RoutePrefix("api/bulkupdate")]
    public class RevisionsController : ApiController
    {
        private const string TableName = "BulkUpdateRequests";

        private readonly ITableEntityConverter<IEnumerable<BulkUpdateRequestTableEntity>, BulkUpdateRequest> _converter =
            new BulkUpdateRequestTableEntityConverter();

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

            return new OkNegotiatedContentResult<BulkUpdateRequest>(_converter.FromTableEntity(result), this);
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] int[] clubIds)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://azuredbtest2.scm.azurewebsites.net/api/")
            };
            var byteArray =
                Encoding.ASCII.GetBytes("$azuredbtest2:Sz4wgrHJd8ePpLxXSQqbuWHyEDqqMjvrEbGnul9XYmtQkvgCymFg5JQwQRk4");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));       

            var guid = SequentialGuidGenerator.NewSequentialId();
            var request = new BulkUpdateRequestTableEntity()
            {
                PartitionKey = guid.ToString(),
                RowKey = guid.ToString(),
                Status = "Queued",
                ClubIds = string.Join(" ", clubIds.Select(id => id.ToString()))
            };

            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference("myqueue");
            queue.CreateIfNotExists();
            var message = new CloudQueueMessage(guid.ToString());

            await AzureTableAdapter.Upsert(request, TableName);

            var url = $"triggeredwebjobs/azuredbtest1/run?arguments={guid}";
            var response = await client.PostAsync(url, null);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                await Task.Delay(500);
                await client.PostAsync(url, null);
            }

            return new OkNegotiatedContentResult<BulkUpdateRequest>(_converter.FromTableEntity(new[] {request}), this);
        }

        [Route("{requestId}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(string requestId)
        {
            var request = await 
                AzureTableAdapter.GetByRowKeyAndPartKey<BulkUpdateRequestTableEntity>(TableName, requestId, requestId);

            if (request.Status == "Queued" || request.Status == "Inprogress")
            {
                return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Request is not finished yet")
                });
            }

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