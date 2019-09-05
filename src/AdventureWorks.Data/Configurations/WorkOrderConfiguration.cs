using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.ToTable("WorkOrder", "Production");

            builder.HasKey(a => a.WorkOrderId);

            builder.HasOne(a => a.Product)
                .WithMany(b=>b.WorkOrders)
                .HasForeignKey(a => a.ProductId);

            builder.HasOne(a => a.ScrapReason)
                .WithMany(b=>b.WorkOrders)
                .HasForeignKey(a => a.ScrapReasonId);
        }
    }
}