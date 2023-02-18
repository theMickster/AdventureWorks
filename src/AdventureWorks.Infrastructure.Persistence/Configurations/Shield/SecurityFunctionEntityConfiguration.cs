using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Shield;

public sealed class SecurityFunctionEntityConfiguration : IEntityTypeConfiguration<SecurityFunctionEntity>
{
    public void Configure(EntityTypeBuilder<SecurityFunctionEntity> builder)
    {
        builder.ToTable("SecurityFunction", "Shield");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("FunctionId");

        builder.Property(a => a.RecordId).HasColumnName("FunctionGuid");

        builder.Property(a => a.Name).HasColumnName("FunctionName");

        builder.Property(a => a.Description).HasColumnName("FunctionDescription");
    }
}
