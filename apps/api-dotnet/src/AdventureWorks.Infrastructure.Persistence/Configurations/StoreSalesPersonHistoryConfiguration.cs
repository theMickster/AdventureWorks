using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class StoreSalesPersonHistoryConfiguration : IEntityTypeConfiguration<StoreSalesPersonHistoryEntity>
{
    public void Configure(EntityTypeBuilder<StoreSalesPersonHistoryEntity> builder)
    {
        builder.ToTable("StoreSalesPersonHistory", "Sales");

        builder.HasKey(a => new { a.BusinessEntityId, a.SalesPersonId, a.StartDate });

        builder.Property(a => a.StartDate).HasColumnType("datetime2");
        builder.Property(a => a.EndDate).HasColumnType("datetime2");
        builder.Property(a => a.ModifiedDate).HasColumnType("datetime2");

        builder.HasOne(a => a.Store)
            .WithMany()
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.SalesPerson)
            .WithMany()
            .HasForeignKey(a => a.SalesPersonId);
    }
}
