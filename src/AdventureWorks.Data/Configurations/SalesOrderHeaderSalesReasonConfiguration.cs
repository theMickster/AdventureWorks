using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class SalesOrderHeaderSalesReasonConfiguration : IEntityTypeConfiguration<SalesOrderHeaderSalesReason>
    {
        public void Configure(EntityTypeBuilder<SalesOrderHeaderSalesReason> builder)
        {
            builder.ToTable("SalesOrderHeaderSalesReason", "Sales");

            builder.HasKey(a => new {a.SalesOrderId, a.SalesReasonId});

            builder.HasOne(a => a.SalesOrder)
                .WithMany(b=> b.SalesOrderHeaderSalesReasons)
                .HasForeignKey(a => a.SalesOrderId);

            builder.HasOne(a => a.SalesReason)
                .WithMany()
                .HasForeignKey(a => a.SalesReasonId);

        }
    }
}