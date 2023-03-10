using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ContactTypeConfiguration : IEntityTypeConfiguration<ContactTypeEntity>
{
    public void Configure(EntityTypeBuilder<ContactTypeEntity> builder)
    {
        builder.ToTable("ContactType", "Person");

        builder.HasKey(a => a.ContactTypeId);
    }
}