﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace azuredbtest1.Common
{
    [DataContract]
    public class BulkUpdateRequest
    {
        [DataMember(Name = "requestId")]
        public Guid RequestId { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "data")]
        public IEnumerable<BulkUpdateData> Data { get; set; }

        [DataMember]
        public string Error { get; set; }
    }
}