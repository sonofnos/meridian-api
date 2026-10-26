using FluentAssertions;
using Meridian.Domain.Accounts;
using Meridian.Domain.Exceptions;

namespace Meridian.UnitTests.Domain;

public class AccountTests
{
    [Fact]
    public void Open_WithValidArgs_CreatesActiveAccount()
    {
        var account = Account.Open("owner1", "Alice Mwangi", "KES", "MRD0000000001");

        account.Status.Should().Be(AccountStatus.Active);
        account.Balance.Should().Be(0);
        account.Currency.Should().Be("KES");
        account.DomainEvents.Should().ContainSingle(e => e is AccountOpenedEvent);
    }

    [Fact]
    public void Credit_PositiveAmount_IncreasesBalance()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");

        account.Credit(500m, "REF-001");

        account.Balance.Should().Be(500m);
        account.DomainEvents.Should().Contain(e => e is AccountCreditedEvent);
    }

    [Fact]
    public void Debit_SufficientFunds_DecreasesBalance()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");
        account.Credit(500m, "REF-001");

        account.Debit(200m, "REF-002");

        account.Balance.Should().Be(300m);
    }

    [Fact]
    public void Debit_InsufficientFunds_ThrowsInsufficientFundsException()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");
        account.Credit(100m, "REF-001");

        var act = () => account.Debit(200m, "REF-002");

        act.Should().Throw<InsufficientFundsException>();
    }

    [Fact]
    public void Debit_ZeroAmount_ThrowsDomainException()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");

        var act = () => account.Debit(0m, "REF-001");

        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void Freeze_ActiveAccount_SetsFrozenStatus()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");

        account.Freeze();

        account.Status.Should().Be(AccountStatus.Frozen);
        account.DomainEvents.Should().Contain(e => e is AccountFrozenEvent);
    }

    [Fact]
    public void Credit_FrozenAccount_ThrowsDomainException()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");
        account.Freeze();

        var act = () => account.Credit(100m, "REF-001");

        act.Should().Throw<DomainException>().WithMessage("*not active*");
    }

    [Fact]
    public void Close_WithZeroBalance_SetsClosedStatus()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");

        account.Close();

        account.Status.Should().Be(AccountStatus.Closed);
        account.DomainEvents.Should().Contain(e => e is AccountClosedEvent);
    }

    [Fact]
    public void Close_WithNonZeroBalance_ThrowsDomainException()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");
        account.Credit(100m, "REF-001");

        var act = () => account.Close();

        act.Should().Throw<DomainException>().WithMessage("*non-zero balance*");
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var account = Account.Open("owner1", "Alice", "KES", "MRD0000000001");
        account.Credit(100m, "REF-001");

        account.ClearDomainEvents();

        account.DomainEvents.Should().BeEmpty();
    }
}
