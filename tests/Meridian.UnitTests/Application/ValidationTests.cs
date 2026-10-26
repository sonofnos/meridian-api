using FluentAssertions;
using FluentValidation;
using Meridian.Application.Accounts.Commands.OpenAccount;
using Meridian.Application.Transactions.Commands.TransferFunds;

namespace Meridian.UnitTests.Application;

public class ValidationTests
{
    [Theory]
    [InlineData("", "Alice", "KES")]
    [InlineData("owner1", "", "KES")]
    [InlineData("owner1", "Alice", "")]
    [InlineData("owner1", "Alice", "XYZ")]
    public void OpenAccountValidator_InvalidCommand_Fails(string ownerId, string ownerName, string currency)
    {
        var validator = new OpenAccountValidator();

        var result = validator.Validate(new OpenAccountCommand(ownerId, ownerName, currency));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OpenAccountValidator_ValidCommand_Passes()
    {
        var validator = new OpenAccountValidator();

        var result = validator.Validate(new OpenAccountCommand("owner1", "Alice", "KES"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void TransferFundsValidator_SameAccounts_Fails()
    {
        var validator = new TransferFundsValidator();
        var id = Guid.NewGuid();

        var result = validator.Validate(new TransferFundsCommand(id, id, 100m, "KES", "REF-001"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("differ"));
    }

    [Fact]
    public void TransferFundsValidator_NegativeAmount_Fails()
    {
        var validator = new TransferFundsValidator();

        var result = validator.Validate(
            new TransferFundsCommand(Guid.NewGuid(), Guid.NewGuid(), -50m, "KES", "REF-001"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("positive"));
    }
}
