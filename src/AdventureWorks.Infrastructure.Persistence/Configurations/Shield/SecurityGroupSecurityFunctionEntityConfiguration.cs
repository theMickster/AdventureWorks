using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Shield;

public sealed class SecurityGroupSecurityFunctionEntityConfiguration : IEntityTypeConfiguration<SecurityGroupSecurityFunctionEntity>
{
    public void Configure(EntityTypeBuilder<SecurityGroupSecurityFunctionEntity> builder)
    {
        builder.ToTable("SecurityGroupSecurityFunction", "Shield");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("SecurityGroupSecurityFunctionId");

        builder.Property(a => a.RecordId).HasColumnName("SecurityGroupSecurityFunctionGuid");

        builder.HasOne(a => a.SecurityGroup)
            .WithMany(b => b.SecurityGroupSecurityFunctionEntities)
            .HasForeignKey(c => c.GroupId);

        builder.HasOne(a => a.SecurityFunction)
            .WithMany(b => b.SecurityGroupSecurityFunctionEntities)
            .HasForeignKey(c => c.FunctionId);
    }
}
