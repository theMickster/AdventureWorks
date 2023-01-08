using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class BusinessEntityContactConfiguration : IEntityTypeConfiguration<BusinessEntityContact>
{
    public void Configure(EntityTypeBuilder<BusinessEntityContact> builder)
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