using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<AddressEntity>
{
    public void Configure(EntityTypeBuilder<AddressEntity> builder)
    {
        builder.ToTable("Address", "Person");

        builder.HasKey(a => a.AddressId);

        builder.HasOne(a => a.StateProvince)
            .WithMany(b => b.Addresses)
            .HasForeignKey(a => a.StateProvinceId);
    }
}