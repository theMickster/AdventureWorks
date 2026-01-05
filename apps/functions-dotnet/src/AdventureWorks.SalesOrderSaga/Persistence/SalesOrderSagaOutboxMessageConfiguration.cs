using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.SalesOrderSaga.Persistence;

/// <summary>Maps the transactional outbox used for terminal saga event delivery.</summary>
public sealed class SalesOrderSagaOutboxMessageConfiguration : IEntityTypeConfiguration<SalesOrderSagaOutboxMessage>
{
    public void Configure(EntityTypeBuilder<SalesOrderSagaOutboxMessage> builder)
    {
        builder.ToTable("SalesOrderSagaOutboxMessage", "dbo");
        builder.HasKey(message => message.MessageId);
        builder.Property(message => message.MessageId).HasMaxLength(180);
        builder.Property(message => message.EventName).HasMaxLength(80);
        builder.Property(message => message.Payload).HasColumnType("nvarchar(max)");
        builder.Property(message => message.LastDispatchError).HasMaxLength(2048);
        builder.HasIndex(message => message.DispatchedAt);
    }
}
