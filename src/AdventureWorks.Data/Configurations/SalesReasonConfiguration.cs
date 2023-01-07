using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class SalesReasonConfiguration : IEntityTypeConfiguration<SalesReason>
{
    public void Configure(EntityTypeBuilder<SalesReason> builder)
    {
        builder.ToTable("SalesReason", "Sales");

        builder.HasKey(a => a.SalesReasonId);
    }
}