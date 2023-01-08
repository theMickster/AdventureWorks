using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class PersonPhoneConfiguration : IEntityTypeConfiguration<PersonPhone>
{
    public void Configure(EntityTypeBuilder<PersonPhone> builder)
    {
        builder.ToTable("PersonPhone", "Person");

        builder.HasKey(a => new {a.BusinessEntityId, a.PhoneNumber, a.PhoneNumberTypeId});

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b=>b.PersonPhones)
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.PhoneNumberType)
            .WithMany(b=>b.PersonPhones)
            .HasForeignKey(a => a.PhoneNumberTypeId);
    }
}