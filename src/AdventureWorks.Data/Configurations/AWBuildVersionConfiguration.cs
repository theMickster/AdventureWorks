using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class AwBuildVersionConfiguration : IEntityTypeConfiguration<AwbuildVersion>
{
    public void Configure(EntityTypeBuilder<AwbuildVersion> builder)
    {
        builder.ToTable("AWBuildVersion", "dbo");

        builder.HasKey(a => a.SystemInformationId);
    }
}