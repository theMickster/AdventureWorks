using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

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
            .WithOne(b => b.UserAccountEntity)
            .HasForeignKey<Person>(c => c.BusinessEntityId);
    }
}
