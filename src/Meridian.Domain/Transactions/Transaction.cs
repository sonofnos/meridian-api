using Meridian.Domain.Common;
using Meridian.Domain.Exceptions;

namespace Meridian.Domain.Transactions;

public sealed class Transaction : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid DebitAccountId { get; private set; }
    public Guid CreditAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public string Reference { get; private set; } = default!;
    public string? Description { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Transaction() { }

    public static Transaction Initiate(
        Guid debitAccountId,
        Guid creditAccountId,
        decimal amount,
        string currency,
        string reference,
        string? description = null)
    {
        if (amount <= 0) throw new DomainException("Transaction amount must be positive.");
        if (debitAccountId == creditAccountId) throw new DomainException("Debit and credit accounts must differ.");

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Reference = reference,
            Description = description,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        tx.RaiseDomainEvent(new TransactionInitiatedEvent(tx.Id, debitAccountId, creditAccountId, amount, currency, reference));
        return tx;
    }

    public void Complete()
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException($"Cannot complete a transaction in status '{Status}'.");

        Status = TransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new TransactionCompletedEvent(Id, DebitAccountId, CreditAccountId, Amount, Currency));
    }

    public void Fail(string reason)
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException($"Cannot fail a transaction in status '{Status}'.");

        Status = TransactionStatus.Failed;
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new TransactionFailedEvent(Id, reason));
    }

    public void Reverse()
    {
        if (Status != TransactionStatus.Completed)
            throw new DomainException("Only completed transactions can be reversed.");

        Status = TransactionStatus.Reversed;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new TransactionReversedEvent(Id, DebitAccountId, CreditAccountId, Amount, Currency));
    }
}
