using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class AddressTypeConfiguration : IEntityTypeConfiguration<AddressTypeEntity>
{
    public void Configure(EntityTypeBuilder<AddressTypeEntity> builder)
    {
        builder.ToTable("AddressType", "Person");
        builder.HasKey(a => a.AddressTypeId);
    }
}