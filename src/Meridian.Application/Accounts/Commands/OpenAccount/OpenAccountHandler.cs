using MediatR;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Accounts;

namespace Meridian.Application.Accounts.Commands.OpenAccount;

public sealed class OpenAccountHandler(
    IAccountRepository accounts,
    IAccountNumberGenerator numberGenerator,
    IUnitOfWork uow) : IRequestHandler<OpenAccountCommand, OpenAccountResult>
{
    public async Task<OpenAccountResult> Handle(OpenAccountCommand cmd, CancellationToken ct)
    {
        var accountNumber = numberGenerator.Generate();
        var account = Account.Open(cmd.OwnerId, cmd.OwnerName, cmd.Currency, accountNumber);

        await accounts.AddAsync(account, ct);
        await uow.SaveChangesAsync(ct);

        return new OpenAccountResult(account.Id, account.AccountNumber, account.Currency);
    }
}
