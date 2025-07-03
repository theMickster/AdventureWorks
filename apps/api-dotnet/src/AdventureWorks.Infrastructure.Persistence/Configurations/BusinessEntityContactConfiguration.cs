using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class BusinessEntityContactConfiguration : IEntityTypeConfiguration<BusinessEntityContactEntity>
{
    public void Configure(EntityTypeBuilder<BusinessEntityContactEntity> builder)
    {
        builder.ToTable("BusinessEntityContact", "Person");

        builder.HasKey(a => new { a.BusinessEntityId, a.PersonId, a.ContactTypeId });

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b => b.BusinessEntityContacts)
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.Person)
            .WithMany(b=>b.BusinessEntityContacts)
            .HasForeignKey(a => a.PersonId);

        builder.HasOne(a => a.ContactType)
            .WithMany(b=>b.BusinessEntityContacts)
            .HasForeignKey(a => a.ContactTypeId);

    }
}