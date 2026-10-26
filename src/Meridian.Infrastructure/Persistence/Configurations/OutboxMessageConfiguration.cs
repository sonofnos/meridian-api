using Meridian.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventType).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Payload).IsRequired();
        builder.HasIndex(m => m.ProcessedAt);
        builder.ToTable("outbox_messages");
    }
}
