using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class SalesPersonConfiguration : IEntityTypeConfiguration<SalesPerson>
{
    public void Configure(EntityTypeBuilder<SalesPerson> builder)
    {
        builder.ToTable("SalesPerson", "Sales");

        builder.HasKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b=>b.SalesPersons)
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.Territory)
            .WithMany(b=>b.SalesTerritorySalesPersons)
            .HasForeignKey(a => a.TerritoryId);
    }
}