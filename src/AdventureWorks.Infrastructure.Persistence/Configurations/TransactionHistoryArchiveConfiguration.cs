using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class TransactionHistoryArchiveConfiguration : IEntityTypeConfiguration<TransactionHistoryArchive>
{
    public void Configure(EntityTypeBuilder<TransactionHistoryArchive> builder)
    {
        builder.ToTable("TransactionHistoryArchive", "Production");

        builder.HasKey(a => a.TransactionId);
    }
}