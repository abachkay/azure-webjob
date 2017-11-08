using System;
using System.Runtime.Serialization;

namespace Usga.Hcs.Core.Repository
{
    /// <summary>    
    /// General information about club revisions. 
    /// </summary>
    [DataContract]
    public class RevisionInfo
    {
        /// <summary>
        /// Golfer id
        /// </summary>
        [DataMember(Name = "GolferID")]
        public string GolferId { get; set; }

        /// <summary>
        /// Club id
        /// </summary>
        [DataMember(Name = "ClubID")]
        public int ClubId { get; set; }

        /// <summary>
        /// Revision effective date
        /// </summary>
        [DataMember(Name = "DateOfRevision")]
        public DateTime DateOfRevision { get; set; }

        /// <summary>
        /// Handicap index display value for 9 hole revision.
        /// </summary>
        [DataMember(Name = "HI9hDisplayValue")]
        public string Hi9hDisplayValue { get; set; }

        /// <summary>
        /// Handicap index display value for 18 hole revision.
        /// </summary>
        [DataMember(Name = "HI18hDisplayValue")]
        public string Hi18hDisplayValue { get; set; }
    }
}