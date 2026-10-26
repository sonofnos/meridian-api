namespace Meridian.Domain.Common;

public abstract record DomainEvent(Guid Id, DateTime OccurredAt)
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow) { }
}
