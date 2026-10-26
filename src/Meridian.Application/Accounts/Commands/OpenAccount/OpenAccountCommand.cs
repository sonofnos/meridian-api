using MediatR;

namespace Meridian.Application.Accounts.Commands.OpenAccount;

public sealed record OpenAccountCommand(
    string OwnerId,
    string OwnerName,
    string Currency) : IRequest<OpenAccountResult>;

public sealed record OpenAccountResult(Guid AccountId, string AccountNumber, string Currency);
