using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Person;

public sealed class PersonTypeEntityConfiguration : IEntityTypeConfiguration<PersonTypeEntity>
{
    public void Configure(EntityTypeBuilder<PersonTypeEntity> builder)
    {
        builder.ToTable("PersonType", "Person");

        builder.HasKey(a => a.PersonTypeId);

        builder.Property(a => a.PersonTypeCode).HasMaxLength(10);

        builder.Property(a => a.PersonTypeName).HasMaxLength(128);

        builder.Property(a => a.PersonTypeName).HasMaxLength(500);

    }
}
