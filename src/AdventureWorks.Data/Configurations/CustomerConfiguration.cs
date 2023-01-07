using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customer", "Sales");

        builder.HasKey(a => a.CustomerId);

        builder.HasOne(a => a.Person)
            .WithMany(b=>b.Customers)
            .HasForeignKey(a => a.PersonId);

        builder.HasOne(a => a.Territory)
            .WithMany(b=>b.Customers)
            .HasForeignKey(a => a.TerritoryId);

        builder.HasOne(a => a.Store)
            .WithMany(b=>b.Customers)
            .HasForeignKey(a => a.StoreId);
    }
}