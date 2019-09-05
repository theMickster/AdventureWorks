using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class AwBuildVersionConfiguration : IEntityTypeConfiguration<AwbuildVersion>
    {
        public void Configure(EntityTypeBuilder<AwbuildVersion> builder)
        {
            builder.ToTable("AWBuildVersion", "dbo");

            builder.HasKey(a => a.SystemInformationId);
        }
    }
}
