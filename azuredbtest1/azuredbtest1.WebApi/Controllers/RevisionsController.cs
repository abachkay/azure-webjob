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
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
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
            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://azuredbtest2.scm.azurewebsites.net/api/")
            };
            var byteArray =
                Encoding.ASCII.GetBytes("$azuredbtest2:Sz4wgrHJd8ePpLxXSQqbuWHyEDqqMjvrEbGnul9XYmtQkvgCymFg5JQwQRk4");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var stringBuilder = new StringBuilder();

            foreach (var clubId in clubIds)
            {
                stringBuilder.Append(clubId+" ");
            }

            var url = "triggeredwebjobs/azuredbtest1/run";//?arguments=" + stringBuilder;

            var response = await client.PostAsync(url, new ObjectContent(typeof(object),new{arguments=clubIds.Select(c => c.ToString()).ToArray()}, new JsonMediaTypeFormatter()));

            return new ResponseMessageResult(response);
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