using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;

namespace azuredbtest1.Common
{
    public class AzureTableAdapter
    {
        public static IEnumerable<BulkUpdateRequestTableEntity> GetStatus()
        {
            var table = tableClient.GetTableReference("BulkUpdateRequests");                        
            var rangeQuery = new TableQuery<BulkUpdateRequestTableEntity>();
            return table.ExecuteQuery(rangeQuery);
        }

        public static IEnumerable<T> GetAll<T>(string tableName)
            where T : ITableEntity, new()
        {
            var table = tableClient.GetTableReference("BulkUpdateRequests");
            var rangeQuery = new TableQuery<T>();
            return table.ExecuteQuery(rangeQuery);
        }

        private static readonly CloudStorageAccount storageAccount = InitAccount();        
        private static readonly CloudTableClient tableClient = InitClient();

        private static CloudStorageAccount InitAccount()
        {
            var sa = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);            
            var tableServicePoint = ServicePointManager.FindServicePoint(sa.TableEndpoint);
            tableServicePoint.UseNagleAlgorithm = false;
            return sa;
        }

        private static CloudTableClient InitClient()
        {
            var tc = storageAccount.CreateCloudTableClient();
            tc.DefaultRequestOptions.PayloadFormat = TablePayloadFormat.JsonNoMetadata;
            return tc;
        }

        public static void CreateTableIfNotExists(string name)
        {
            // Retrieve a reference to the table.
            var table = tableClient.GetTableReference(name);

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
        }

        public static async Task<T> GetByRowKeyAndPartKey<T>(string tableName, string partKey, string rowKey) where T : ITableEntity
        {
            try
            {
                // Create the CloudTable object that represents the  table.
                var table = tableClient.GetTableReference(tableName);
                // Create a retrieve operation that takes a customer entity.
                var retrieveOperation = TableOperation.Retrieve<T>(partKey, rowKey);
                // Execute the retrieve operation.
                var retrievedResult = table.ExecuteAsync(retrieveOperation);
                var res = await retrievedResult;
                return (T)res.Result;
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    CreateTableIfNotExists(tableName);
                    throw new ApplicationException("Table was just created. Please try again");
                }
                throw;
            }
        }

        public static async Task<List<T>> GetByRowKeyAndPartKey<T>(string tableName, string partKey, string[] rowKeys) where T : class, ITableEntity, new()
        {
            if (rowKeys.Length > 10)
            {
                throw new Exception("Use GetByPartKey for such amount of keys");
            }
            var filter = string.Join(" or ", rowKeys.Select(q => string.Format("RowKey eq '{0}'", q)));
            return await GetByPartKey<T>(tableName, partKey, filter, null);
        }

        public static async Task<List<T>> GetByRowKeyAndPartKey<T>(string tableName, IEnumerable<Tuple<string, string>> partRowKeys) where T : class, ITableEntity, new()
        {
            if (partRowKeys == null || !partRowKeys.Any())
            {
                throw new Exception("keys expected");
            }

            var filter = string.Join(" or ", partRowKeys.Select(q => string.Format("(PartitionKey eq '{0}' and RowKey eq '{1}')", q.Item1, q.Item2)));
            return await Query<T>(tableName, filter);
        }

        public static async Task<List<T>> GetByPartKey<T>(string tableName, string partKey, string filter = null, int? take = 1000) where T : class, ITableEntity, new()
        {
            // Initialize a default TableQuery to retrieve all the entities in the table.
            var query = string.Format("(PartitionKey eq '{0}')", partKey);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query += " and (" + filter + ")";
            }

            return await Query<T>(tableName, query, take);
        }

        public static async Task TruncateTable(string tableName)
        {
            // Retrieve a reference to the table.
            var table = tableClient.GetTableReference(tableName);
            await table.DeleteAsync();
            CreateTableIfNotExists(tableName);
        }

        public static async Task<List<T>> Query<T>(string tableName, string query, int? take = 1000) where T : class, ITableEntity, new()
        {
            var tableQuery = new TableQuery<T>().Where(query);
            if (take.HasValue)
            {
                tableQuery = tableQuery.Take(take.Value);
            }
            var table = tableClient.GetTableReference(tableName);

            try
            {
                // Initialize the continuation token to null to start from the beginning of the table.
                TableContinuationToken continuationToken = null;
                var results = new List<T>();
                do
                {
                    // Retrieve a segment (up to 1,000 entities).
                    var tableQueryResult =
                        await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                    // Assign the new continuation token to tell the service where to
                    // continue on the next iteration (or null if it has reached the end).
                    continuationToken = tableQueryResult.ContinuationToken;
                    results.AddRange(tableQueryResult.Results);
                    // Loop until a null continuation token is received, indicating the end of the table.
                }
                while (continuationToken != null && (!take.HasValue || results.Count() < take));
                return results;
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    CreateTableIfNotExists(tableName);
                    throw new ApplicationException("Table was just created. Please try again");
                }
                throw;
            }
        }

        public static async Task<string> Upsert(ITableEntity entity, string tableName)
        {
            try
            {
                // Create the InsertOrReplace TableOperation.
                var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);
                var table = tableClient.GetTableReference(tableName);
                // Execute the operation.
                var res = await table.ExecuteAsync(insertOrReplaceOperation);
                return res.Etag;
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    CreateTableIfNotExists(tableName);
                    throw new ApplicationException("Table was just created. Please try again");
                }
                throw;
            }
        }

        public static async Task BatchUpsert(IEnumerable<ITableEntity> entities, string tableName)
        {
            try
            {
                var table = tableClient.GetTableReference(tableName);
                // Create the batch operation.
                var batchOperation = new TableBatchOperation();
                foreach (var ent in entities)
                {
                    batchOperation.InsertOrReplace(ent);
                }
                // Execute the batch operation.
                var res = await table.ExecuteBatchAsync(batchOperation);
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    CreateTableIfNotExists(tableName);
                    throw new ApplicationException("Table was just created. Please try again");
                }
                throw;
            }
        }

        public static async Task BatchUpsertAndDelete(IEnumerable<ITableEntity> entitiesToUpsert, IEnumerable<ITableEntity> entitiesToDelete, string tableName)
        {
            try
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var entity in entitiesToUpsert)
                {
                    batchOperation.InsertOrReplace(entity);
                }

                foreach (var entity in entitiesToDelete)
                {
                    batchOperation.Delete(entity);
                }

                var res = await table.ExecuteBatchAsync(batchOperation);
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    CreateTableIfNotExists(tableName);
                    throw new ApplicationException("Table was just created. Please try again");
                }
                throw;
            }
        }

        public static async Task UpsertMany(IEnumerable<ITableEntity> entities, string tableName)
        {
            try
            {
                var table = tableClient.GetTableReference(tableName);
                // Create the InsertOrReplace TableOperation.
                // Execute the operation.


                var tasks = new List<Task>();
                foreach (var entity in entities)
                {
                    var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);
                    tasks.Add(table.ExecuteAsync(insertOrReplaceOperation));
                }
                await Task.WhenAll(tasks);

                // Execute the batch operation.
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    CreateTableIfNotExists(tableName);
                    throw new ApplicationException("Table was just created. Please try again");
                }
                throw;
            }
        }

        public static async Task<T> Delete<T>(string rowKey, string partKey, string tableName) where T : ITableEntity
        {
            var deleteEntity = await GetByRowKeyAndPartKey<T>(tableName, partKey, rowKey);

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {
                await Delete(deleteEntity, tableName);
            }

            return deleteEntity;
        }

        public static async Task<bool> Delete<T>(T deleteEntity, string tableName) where T : ITableEntity
        {
            var table = tableClient.GetTableReference(tableName);
            var deleteOperation = TableOperation.Delete(deleteEntity);
            // Execute the operation.
            var res = await table.ExecuteAsync(deleteOperation);
            return res.HttpStatusCode == 200;
        }

        public static async Task Delete(string partKey, string tableName)
        {
            var entities = await GetByPartKey<TableEntity>(tableName, partKey, null, int.MaxValue);
            var tasks = new List<Task>();
            entities.ForEach(q => tasks.Add(Delete(q, tableName)));
            await Task.WhenAll(tasks);
        }

        public static async Task BatchDelete(string[] rowKeys, string partKey, string tableName)
        {
            await BatchDelete(rowKeys.Select(q => new Tuple<string, string>(partKey, q)), tableName);
        }

        public static async Task BatchDelete(IEnumerable<ITableEntity> entities, string tableName)
        {
            try
            {
                var table = tableClient.GetTableReference(tableName);
                // Create the batch operation.
                var batchOperation = new TableBatchOperation();
                foreach (var entity in entities)
                {
                    entity.ETag = "*";
                    batchOperation.Delete(entity);
                }
                // Execute the batch operation.
                var res = await table.ExecuteBatchAsync(batchOperation);
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    CreateTableIfNotExists(tableName);
                    throw new ApplicationException("Table was just created. Please try again");
                }
                throw;
            }
        }

        /// <summary>
        /// Removes collections
        /// </summary>
        /// <param name="records">PartKey - first! RowKey - second</param>
        /// <param name="tableName">Table containing records to be remopved</param>
        /// <returns></returns>
        public static async Task BatchDelete(IEnumerable<Tuple<string, string>> records, string tableName)
        {
            await BatchDelete(records.Select(q => new TableEntity { PartitionKey = q.Item1, RowKey = q.Item2 }), tableName);
        }
    }
}
