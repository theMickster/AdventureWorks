using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Shield;

public sealed class SecurityGroupUserAccountEntityConfiguration : IEntityTypeConfiguration<SecurityGroupUserAccountEntity>
{
    public void Configure(EntityTypeBuilder<SecurityGroupUserAccountEntity> builder)
    {
        builder.ToTable("SecurityGroupUserAccount", "Shield");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("SecurityGroupUserAccountId");

        builder.Property(a => a.RecordId).HasColumnName("SecurityGroupUserAccountGuid");

        builder.HasOne(a => a.UserAccount)
            .WithMany(b => b.SecurityGroupUserAccountEntities)
            .HasForeignKey(c => c.BusinessEntityId);

        builder.HasOne(a => a.BusinessEntity)
            .WithMany()
            .HasForeignKey(c => c.BusinessEntityId);
        
        builder.HasOne(a => a.SecurityGroup)
            .WithMany(b => b.SecurityGroupUserAccountEntities)
            .HasForeignKey(c => c.GroupId);
    }
}
