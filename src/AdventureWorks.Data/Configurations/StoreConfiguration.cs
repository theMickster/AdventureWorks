using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Store", "Sales");

            builder.HasKey(a => a.BusinessEntityId);

            builder.HasOne(a => a.BusinessEntity)
                .WithMany(b => b.Stores)
                .HasForeignKey(a => a.BusinessEntityId);

            builder.HasOne(a => a.SalesPerson)
                .WithMany()
                .HasForeignKey(a => a.SalesPersonId);
        }
    }
}