using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class EmailAddressConfiguration : IEntityTypeConfiguration<EmailAddressEntity>
{
    public void Configure(EntityTypeBuilder<EmailAddressEntity> builder)
    {
        builder.ToTable("EmailAddress", "Person");

        builder.Property(x => x.EmailAddressName).HasColumnName("EmailAddress");

        builder.HasKey(a => a.EmailAddressId);

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b=>b.EmailAddresses)
            .HasForeignKey(a => a.BusinessEntityId);
    }
}