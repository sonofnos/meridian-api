using MediatR;
using Meridian.Domain.Accounts;
using Meridian.Domain.Exceptions;

namespace Meridian.Application.Accounts.Queries.GetAccount;

public sealed class GetAccountHandler(IAccountRepository accounts)
    : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery query, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(query.AccountId, ct)
            ?? throw new AccountNotFoundException(query.AccountId);

        return new AccountDto(
            account.Id, account.OwnerId, account.OwnerName,
            account.AccountNumber, account.Currency, account.Balance,
            account.Status, account.CreatedAt);
    }
}
