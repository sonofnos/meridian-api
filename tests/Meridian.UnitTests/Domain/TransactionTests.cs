using FluentAssertions;
using Meridian.Domain.Exceptions;
using Meridian.Domain.Transactions;

namespace Meridian.UnitTests.Domain;

public class TransactionTests
{
    private static readonly Guid Debit = Guid.NewGuid();
    private static readonly Guid Credit = Guid.NewGuid();

    [Fact]
    public void Initiate_ValidArgs_CreatesPendingTransaction()
    {
        var tx = Transaction.Initiate(Debit, Credit, 100m, "KES", "REF-001");

        tx.Status.Should().Be(TransactionStatus.Pending);
        tx.Amount.Should().Be(100m);
        tx.DomainEvents.Should().ContainSingle(e => e is TransactionInitiatedEvent);
    }

    [Fact]
    public void Initiate_ZeroAmount_ThrowsDomainException()
    {
        var act = () => Transaction.Initiate(Debit, Credit, 0m, "KES", "REF-001");

        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void Initiate_SameAccounts_ThrowsDomainException()
    {
        var sameId = Guid.NewGuid();
        var act = () => Transaction.Initiate(sameId, sameId, 100m, "KES", "REF-001");

        act.Should().Throw<DomainException>().WithMessage("*differ*");
    }

    [Fact]
    public void Complete_PendingTransaction_SetsCompletedStatus()
    {
        var tx = Transaction.Initiate(Debit, Credit, 100m, "KES", "REF-001");

        tx.Complete();

        tx.Status.Should().Be(TransactionStatus.Completed);
        tx.CompletedAt.Should().NotBeNull();
        tx.DomainEvents.Should().Contain(e => e is TransactionCompletedEvent);
    }

    [Fact]
    public void Fail_PendingTransaction_SetsFailedStatus()
    {
        var tx = Transaction.Initiate(Debit, Credit, 100m, "KES", "REF-001");

        tx.Fail("Insufficient funds");

        tx.Status.Should().Be(TransactionStatus.Failed);
        tx.FailureReason.Should().Be("Insufficient funds");
    }

    [Fact]
    public void Reverse_CompletedTransaction_SetsReversedStatus()
    {
        var tx = Transaction.Initiate(Debit, Credit, 100m, "KES", "REF-001");
        tx.Complete();

        tx.Reverse();

        tx.Status.Should().Be(TransactionStatus.Reversed);
        tx.DomainEvents.Should().Contain(e => e is TransactionReversedEvent);
    }

    [Fact]
    public void Reverse_PendingTransaction_ThrowsDomainException()
    {
        var tx = Transaction.Initiate(Debit, Credit, 100m, "KES", "REF-001");

        var act = () => tx.Reverse();

        act.Should().Throw<DomainException>().WithMessage("*completed*");
    }

    [Fact]
    public void Complete_AlreadyCompleted_ThrowsDomainException()
    {
        var tx = Transaction.Initiate(Debit, Credit, 100m, "KES", "REF-001");
        tx.Complete();

        var act = () => tx.Complete();

        act.Should().Throw<DomainException>();
    }
}
