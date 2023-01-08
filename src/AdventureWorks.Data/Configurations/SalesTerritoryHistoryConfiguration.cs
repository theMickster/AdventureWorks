using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class SalesTerritoryHistoryConfiguration : IEntityTypeConfiguration<SalesTerritoryHistory>
{
    public void Configure(EntityTypeBuilder<SalesTerritoryHistory> builder)
    {
        builder.ToTable("SalesTerritoryHistory", "Sales");

        builder.HasKey(a => new {a.BusinessEntityId, a.TerritoryId, a.StartDate});

        builder.HasOne(a => a.BusinessEntity)
            .WithMany()
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.TerritoryEntity)
            .WithMany()
            .HasForeignKey(a => a.TerritoryId);

    }
}