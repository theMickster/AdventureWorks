using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class WorkOrderRoutingConfiguration : IEntityTypeConfiguration<WorkOrderRouting>
{
    public void Configure(EntityTypeBuilder<WorkOrderRouting> builder)
    {
        builder.ToTable("WorkOrderRouting", "Production");

        builder.HasKey(a => new {a.WorkOrderId, a.ProductId, a.OperationSequence});

        builder.HasOne(a => a.WorkOrder)
            .WithMany(b=>b.WorkOrderRoutings)
            .HasForeignKey(a => a.WorkOrderId);

        builder.HasOne(a => a.Product)
            .WithMany(b=>b.WorkOrderRoutings)
            .HasForeignKey(a => a.ProductId);

        builder.HasOne(a => a.Location)
            .WithMany(b=>b.WorkOrderRoutings)
            .HasForeignKey(a => a.LocationId);

    }
}