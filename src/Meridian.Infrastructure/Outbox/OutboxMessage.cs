using Meridian.Domain.Common;
using System.Text.Json;

namespace Meridian.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage From(DomainEvent domainEvent) => new()
    {
        Id = Guid.NewGuid(),
        EventType = domainEvent.GetType().AssemblyQualifiedName!,
        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
        CreatedAt = DateTime.UtcNow,
    };

    public void MarkProcessed() => ProcessedAt = DateTime.UtcNow;

    public void MarkFailed(string error)
    {
        Error = error;
        ProcessedAt = DateTime.UtcNow;
    }
}
