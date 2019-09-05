using System;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class AwbuildVersion : BaseEntity, IAggregateRoot
    {
        public byte SystemInformationId { get; set; }
        public string DatabaseVersion { get; set; }
        public DateTime VersionDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
