using MediatR;
using Meridian.Domain.Accounts;

namespace Meridian.Application.Accounts.Queries.GetAccount;

public sealed record GetAccountQuery(Guid AccountId) : IRequest<AccountDto>;

public sealed record AccountDto(
    Guid Id,
    string OwnerId,
    string OwnerName,
    string AccountNumber,
    string Currency,
    decimal Balance,
    AccountStatus Status,
    DateTime CreatedAt);
