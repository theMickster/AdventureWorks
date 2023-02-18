using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class EmailAddressConfiguration : IEntityTypeConfiguration<EmailAddress>
{
    public void Configure(EntityTypeBuilder<EmailAddress> builder)
    {
        builder.ToTable("EmailAddress", "Person");

        builder.Property(x => x.EmailAddressName).HasColumnName("EmailAddress");

        builder.HasKey(a => a.EmailAddressId);

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b=>b.EmailAddresses)
            .HasForeignKey(a => a.BusinessEntityId);
    }
}