using System;

namespace AdventureWorks.Domain.Entities
{
    public class AwbuildVersion : BaseEntity
    {
        public byte SystemInformationId { get; set; }
        public string DatabaseVersion { get; set; }
        public DateTime VersionDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
