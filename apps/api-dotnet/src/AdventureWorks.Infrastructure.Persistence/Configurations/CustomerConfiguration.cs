using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("Customer", "Sales");

        builder.HasKey(a => a.CustomerId);

        builder.HasOne(a => a.Person)
            .WithMany(b=>b.Customers)
            .HasForeignKey(a => a.PersonId);

        builder.HasOne(a => a.TerritoryEntity)
            .WithMany(b=>b.Customers)
            .HasForeignKey(a => a.TerritoryId);

        builder.HasOne(a => a.StoreEntity)
            .WithMany(b=>b.Customers)
            .HasForeignKey(a => a.StoreId);
    }
}