using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class PhoneNumberTypeConfiguration : IEntityTypeConfiguration<PhoneNumberTypeEntity>
{
    public void Configure(EntityTypeBuilder<PhoneNumberTypeEntity> builder)
    {
        builder.ToTable("PhoneNumberType", "Person");

        builder.HasKey(a => a.PhoneNumberTypeId);

    }
}