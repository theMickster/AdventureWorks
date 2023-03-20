using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Shield;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccountEntity>
{
    public void Configure(EntityTypeBuilder<UserAccountEntity> builder)
    {
        builder.ToTable("UserAccount", "Shield");

        builder.HasKey(a => a.BusinessEntityId);

        builder.Property(b => b.RecordId).HasColumnName("rowguid");

        builder.Property(b => b.PasswordHash).HasColumnName("PasswordHash");

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b => b.UserAccounts)
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.Person)
            .WithOne(b => b.UserAccount)
            .HasForeignKey<PersonEntity>(c => c.BusinessEntityId);

        builder.HasOne(a => a.EmailAddress)
            .WithMany()
            .HasForeignKey(x => x.PrimaryEmailAddressId)
            .HasPrincipalKey(y => y.EmailAddressId);

    }
}
