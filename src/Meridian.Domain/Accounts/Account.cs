using Meridian.Domain.Common;
using Meridian.Domain.Exceptions;

namespace Meridian.Domain.Accounts;

public sealed class Account : AggregateRoot
{
    public Guid Id { get; private set; }
    public string OwnerId { get; private set; } = default!;
    public string OwnerName { get; private set; } = default!;
    public string AccountNumber { get; private set; } = default!;
    public string Currency { get; private set; } = default!;
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Account() { }

    public static Account Open(string ownerId, string ownerName, string currency, string accountNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            OwnerName = ownerName,
            AccountNumber = accountNumber,
            Currency = currency.ToUpperInvariant(),
            Balance = 0m,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };

        account.RaiseDomainEvent(new AccountOpenedEvent(account.Id, account.OwnerId, account.Currency));
        return account;
    }

    public void Credit(decimal amount, string reference)
    {
        EnsureActive();
        if (amount <= 0) throw new DomainException("Credit amount must be positive.");

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new AccountCreditedEvent(Id, amount, Balance, reference));
    }

    public void Debit(decimal amount, string reference)
    {
        EnsureActive();
        if (amount <= 0) throw new DomainException("Debit amount must be positive.");
        if (Balance < amount) throw new InsufficientFundsException(Id, amount, Balance);

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new AccountDebitedEvent(Id, amount, Balance, reference));
    }

    public void Freeze()
    {
        if (Status != AccountStatus.Active)
            throw new DomainException($"Cannot freeze account in status '{Status}'.");

        Status = AccountStatus.Frozen;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new AccountFrozenEvent(Id));
    }

    public void Unfreeze()
    {
        if (Status != AccountStatus.Frozen)
            throw new DomainException($"Cannot unfreeze account in status '{Status}'.");

        Status = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status == AccountStatus.Closed)
            throw new DomainException("Account is already closed.");
        if (Balance > 0)
            throw new DomainException("Cannot close an account with a non-zero balance.");

        Status = AccountStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new AccountClosedEvent(Id));
    }

    private void EnsureActive()
    {
        if (Status != AccountStatus.Active)
            throw new DomainException($"Account {Id} is not active (status: {Status}).");
    }
}
