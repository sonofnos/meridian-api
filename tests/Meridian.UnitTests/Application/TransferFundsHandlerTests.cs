using FluentAssertions;
using Meridian.Application.Common.Interfaces;
using Meridian.Application.Transactions.Commands.TransferFunds;
using Meridian.Domain.Accounts;
using Meridian.Domain.Exceptions;
using Meridian.Domain.Transactions;
using Moq;

namespace Meridian.UnitTests.Application;

public class TransferFundsHandlerTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private TransferFundsHandler CreateHandler() =>
        new(_accounts.Object, _transactions.Object, _uow.Object);

    private static Account ActiveAccount(string number = "MRD0000000001")
    {
        var acc = Account.Open("owner1", "Alice", "KES", number);
        acc.Credit(1000m, "SEED");
        acc.ClearDomainEvents();
        return acc;
    }

    [Fact]
    public async Task Handle_ValidTransfer_CompletesAndReturnsResult()
    {
        var debit = ActiveAccount("MRD0000000001");
        var credit = ActiveAccount("MRD0000000002");

        _transactions.Setup(r => r.GetByReferenceAsync("REF-001", default))
            .ReturnsAsync((Transaction?)null);
        _accounts.Setup(r => r.GetByIdAsync(debit.Id, default)).ReturnsAsync(debit);
        _accounts.Setup(r => r.GetByIdAsync(credit.Id, default)).ReturnsAsync(credit);

        var cmd = new TransferFundsCommand(debit.Id, credit.Id, 300m, "KES", "REF-001");
        var result = await CreateHandler().Handle(cmd, default);

        result.Amount.Should().Be(300m);
        result.Reference.Should().Be("REF-001");
        debit.Balance.Should().Be(700m);
        credit.Balance.Should().Be(1300m);
    }

    [Fact]
    public async Task Handle_InsufficientFunds_ThrowsInsufficientFundsException()
    {
        var debit = ActiveAccount("MRD0000000001");
        var credit = ActiveAccount("MRD0000000002");

        _transactions.Setup(r => r.GetByReferenceAsync("REF-002", default))
            .ReturnsAsync((Transaction?)null);
        _accounts.Setup(r => r.GetByIdAsync(debit.Id, default)).ReturnsAsync(debit);
        _accounts.Setup(r => r.GetByIdAsync(credit.Id, default)).ReturnsAsync(credit);

        var cmd = new TransferFundsCommand(debit.Id, credit.Id, 9999m, "KES", "REF-002");
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<InsufficientFundsException>();
    }

    [Fact]
    public async Task Handle_CompletedDuplicateReference_ReturnsIdempotentResult()
    {
        var debit = ActiveAccount("MRD0000000001");
        var credit = ActiveAccount("MRD0000000002");

        var existing = Transaction.Initiate(debit.Id, credit.Id, 100m, "KES", "REF-DUP");
        existing.Complete();

        _transactions.Setup(r => r.GetByReferenceAsync("REF-DUP", default)).ReturnsAsync(existing);

        var cmd = new TransferFundsCommand(debit.Id, credit.Id, 100m, "KES", "REF-DUP");
        var result = await CreateHandler().Handle(cmd, default);

        result.Reference.Should().Be("REF-DUP");
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_DebitAccountNotFound_ThrowsAccountNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _transactions.Setup(r => r.GetByReferenceAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Transaction?)null);
        _accounts.Setup(r => r.GetByIdAsync(missingId, default)).ReturnsAsync((Account?)null);

        var cmd = new TransferFundsCommand(missingId, Guid.NewGuid(), 100m, "KES", "REF-X");
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<AccountNotFoundException>();
    }
}
