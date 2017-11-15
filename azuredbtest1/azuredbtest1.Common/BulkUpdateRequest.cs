using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using azuredbtest1.Common;

namespace Usga.Hcs.Common.Model
{
    [DataContract]
    public class BulkUpdateRequest
    {
        [DataMember(Name = "requestId")]
        public Guid RequestId { get; set; }

        [DataMember(Name = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public BulkUpdateStatus Status { get; set; }

        public DateTime? DateOfStart { get; set; }

        public IEnumerable<int> ClubIds { get; set; }

        [DataMember(Name = "data")]
        public IEnumerable<BulkUpdateData> Data { get; set; }

        [DataMember]
        public string Error { get; set; }

     
    }
}