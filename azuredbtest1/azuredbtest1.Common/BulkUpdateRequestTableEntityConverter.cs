using System;
using System.Collections.Generic;
using System.Linq;

namespace azuredbtest1.Common
{
    public class BulkUpdateRequestTableEntityConverter: ITableEntityConverter<IEnumerable<BulkUpdateRequestTableEntity>, BulkUpdateRequest>
    {
        public BulkUpdateRequest FromTableEntity(IEnumerable<BulkUpdateRequestTableEntity> arg)
        {
            var requestsTableEntities = arg.Where(a => a.PartitionKey == a.RowKey);
            if (requestsTableEntities.Count() != 1)
            {
                throw new ArgumentException("Arg should have one request with PartitionKey equals RowKey");
            }
            var requestsTableEntity = requestsTableEntities.First();
            var data = arg.Where(a => a.PartitionKey != a.RowKey).Select(r => new BulkUpdateData()
            {
                ClubId = r.ClubId.Value,
                DateOfRevision = r.DateOfRevision.Value,
                Success = r.Status == "Done",
                GolferId = r.GolferId,
                Hi9hDisplayValue = r.Hi9HDisplayValue,
                Hi18hDisplayValue = r.Hi18HDisplayValue,
                Error = r.Error
            });

            if (!data.Any())
            {
                data = null;
                
            }

            return new BulkUpdateRequest()
            {
                RequestId = Guid.Parse(requestsTableEntity.PartitionKey),
                Status = requestsTableEntity.Status,
                Data = data,
                Error = requestsTableEntity.Error
            };            
        }
    }
}