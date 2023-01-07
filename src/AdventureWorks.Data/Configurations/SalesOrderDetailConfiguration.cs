using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class SalesOrderDetailConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
    {
        builder.ToTable("SalesOrderDetail", "Sales");

        builder.HasKey(a => new {a.SalesOrderId, a.SalesOrderDetailId});

        builder.HasOne(a => a.SalesOrder)
            .WithMany(b => b.SalesOrderDetails)
            .HasForeignKey(a => a.SalesOrderId);

        builder.HasOne(a => a.SpecialOfferProduct)
            .WithMany(b => b.SalesOrderDetail)
            .HasForeignKey(a => new {a.SpecialOfferId, a.ProductId});

    }
}