using Meridian.Domain.Common;

namespace Meridian.Domain.Transactions;

public sealed record TransactionInitiatedEvent(
    Guid TransactionId, Guid DebitAccountId, Guid CreditAccountId,
    decimal Amount, string Currency, string Reference) : DomainEvent;

public sealed record TransactionCompletedEvent(
    Guid TransactionId, Guid DebitAccountId, Guid CreditAccountId,
    decimal Amount, string Currency) : DomainEvent;

public sealed record TransactionFailedEvent(Guid TransactionId, string Reason) : DomainEvent;

public sealed record TransactionReversedEvent(
    Guid TransactionId, Guid DebitAccountId, Guid CreditAccountId,
    decimal Amount, string Currency) : DomainEvent;
