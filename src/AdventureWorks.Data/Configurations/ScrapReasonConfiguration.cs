using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ScrapReasonConfiguration : IEntityTypeConfiguration<ScrapReason>
    {
        public void Configure(EntityTypeBuilder<ScrapReason> builder)
        {
            builder.ToTable("ScrapReason", "Production");

            builder.HasKey(a => a.ScrapReasonId);
        }
    }
}