using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class SalesPersonConfiguration : IEntityTypeConfiguration<SalesPersonEntity>
{
    public void Configure(EntityTypeBuilder<SalesPersonEntity> builder)
    {
        builder.ToTable("SalesPerson", "Sales");

        builder.HasKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.Employee)
            .WithMany(b=> b.SalesPersons)
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.SalesTerritory)
            .WithMany(b=>b.SalesPeople)
            .HasForeignKey(a => a.TerritoryId);
    }
}