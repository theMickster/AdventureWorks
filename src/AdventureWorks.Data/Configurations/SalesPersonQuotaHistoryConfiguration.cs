using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class SalesPersonQuotaHistoryConfiguration : IEntityTypeConfiguration<SalesPersonQuotaHistory>
    {
        public void Configure(EntityTypeBuilder<SalesPersonQuotaHistory> builder)
        {
            builder.ToTable("SalesPersonQuotaHistory", "Sales");

            builder.HasKey(a => new {a.BusinessEntityId, a.QuotaDate});

            builder.HasOne(a => a.BusinessEntity)
                .WithMany()
                .HasForeignKey(a => a.BusinessEntityId);
        }
    }
}