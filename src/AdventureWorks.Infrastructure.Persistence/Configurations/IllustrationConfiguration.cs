using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class IllustrationConfiguration : IEntityTypeConfiguration<Illustration>
{
    public void Configure(EntityTypeBuilder<Illustration> builder)
    {
        builder.ToTable("Illustration", "Production");

        builder.HasKey(a => a.IllustrationId);
    }
}