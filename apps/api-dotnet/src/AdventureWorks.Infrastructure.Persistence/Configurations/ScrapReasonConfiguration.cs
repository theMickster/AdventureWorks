using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ScrapReasonConfiguration : IEntityTypeConfiguration<ScrapReason>
{
    public void Configure(EntityTypeBuilder<ScrapReason> builder)
    {
        builder.ToTable("ScrapReason", "Production");

        builder.HasKey(a => a.ScrapReasonId);
    }
}