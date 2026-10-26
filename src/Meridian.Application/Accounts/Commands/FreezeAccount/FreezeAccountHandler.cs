using MediatR;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Accounts;
using Meridian.Domain.Exceptions;

namespace Meridian.Application.Accounts.Commands.FreezeAccount;

public sealed class FreezeAccountHandler(IAccountRepository accounts, IUnitOfWork uow)
    : IRequestHandler<FreezeAccountCommand>
{
    public async Task Handle(FreezeAccountCommand cmd, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(cmd.AccountId, ct)
            ?? throw new AccountNotFoundException(cmd.AccountId);

        account.Freeze();
        accounts.Update(account);
        await uow.SaveChangesAsync(ct);
    }
}

public sealed class UnfreezeAccountHandler(IAccountRepository accounts, IUnitOfWork uow)
    : IRequestHandler<UnfreezeAccountCommand>
{
    public async Task Handle(UnfreezeAccountCommand cmd, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(cmd.AccountId, ct)
            ?? throw new AccountNotFoundException(cmd.AccountId);

        account.Unfreeze();
        accounts.Update(account);
        await uow.SaveChangesAsync(ct);
    }
}
