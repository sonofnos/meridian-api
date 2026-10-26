using FluentAssertions;
using Meridian.Application.Accounts.Commands.OpenAccount;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Accounts;
using Moq;

namespace Meridian.UnitTests.Application;

public class OpenAccountHandlerTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IAccountNumberGenerator> _generator = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private OpenAccountHandler CreateHandler() =>
        new(_accounts.Object, _generator.Object, _uow.Object);

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndPersistsAccount()
    {
        _generator.Setup(g => g.Generate()).Returns("MRD0000000001");
        Account? captured = null;
        _accounts.Setup(r => r.AddAsync(It.IsAny<Account>(), default))
            .Callback<Account, CancellationToken>((a, _) => captured = a)
            .Returns(Task.CompletedTask);

        var cmd = new OpenAccountCommand("owner1", "Alice Mwangi", "KES");
        var result = await CreateHandler().Handle(cmd, default);

        result.AccountNumber.Should().Be("MRD0000000001");
        result.Currency.Should().Be("KES");
        captured.Should().NotBeNull();
        captured!.Status.Should().Be(AccountStatus.Active);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        _generator.Setup(g => g.Generate()).Returns("MRD0000000002");

        await CreateHandler().Handle(new OpenAccountCommand("owner2", "Bob", "USD"), default);

        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
