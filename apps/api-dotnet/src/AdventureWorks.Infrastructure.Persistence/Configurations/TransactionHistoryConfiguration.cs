using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class TransactionHistoryConfiguration : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.ToTable("TransactionHistory", "Production");

        builder.HasKey(a => a.TransactionId);

        builder.HasOne(a => a.Product)
            .WithMany(b=>b.TransactionHistory)
            .HasForeignKey(a => a.ProductId);
    }
}