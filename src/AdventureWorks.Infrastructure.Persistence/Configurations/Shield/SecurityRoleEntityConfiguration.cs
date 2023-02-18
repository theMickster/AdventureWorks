using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Shield;

public sealed class SecurityRoleEntityConfiguration : IEntityTypeConfiguration<SecurityRoleEntity>
{
    public void Configure(EntityTypeBuilder<SecurityRoleEntity> builder)
    {
        builder.ToTable("SecurityRole", "Shield");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("RoleId");

        builder.Property(a => a.RecordId).HasColumnName("RoleGuid");

        builder.Property(a => a.Name).HasColumnName("RoleName");

        builder.Property(a => a.Description).HasColumnName("RoleDescription");
    }
}
