using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Shield;

public sealed class SecurityGroupEntityConfiguration : IEntityTypeConfiguration<SecurityGroupEntity>
{
    public void Configure(EntityTypeBuilder<SecurityGroupEntity> builder)
    {
        builder.ToTable("SecurityGroup", "Shield");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("GroupId");

        builder.Property(a => a.RecordId).HasColumnName("GroupGuid");

        builder.Property(a => a.Name).HasColumnName("GroupName");

        builder.Property(a => a.Description).HasColumnName("GroupDescription");
    }
}
