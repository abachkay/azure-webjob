using System.Runtime.Serialization;
using Usga.Hcs.Core.Repository;

namespace Usga.Hcs.Common.Model
{
    [DataContract]
    public class BulkUpdateData: RevisionInfo
    {
        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public bool? Success { get; set; }
    }
}
