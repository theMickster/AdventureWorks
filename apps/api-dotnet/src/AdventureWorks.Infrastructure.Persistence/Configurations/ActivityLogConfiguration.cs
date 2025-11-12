using AdventureWorks.Domain.Entities.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLogEntity>
{
    public void Configure(EntityTypeBuilder<ActivityLogEntity> builder)
    {
        builder.ToTable("ActivityLog", "dbo");
        builder.HasKey(a => a.ActivityLogId);
        builder.Property(a => a.ActivityLogId).ValueGeneratedOnAdd();
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.UserName).IsRequired().HasMaxLength(256);
        builder.Property(a => a.Timestamp).HasColumnType("datetime2");
    }
}
