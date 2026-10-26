using MediatR;

namespace Meridian.Application.Accounts.Commands.FreezeAccount;

public sealed record FreezeAccountCommand(Guid AccountId) : IRequest;

public sealed record UnfreezeAccountCommand(Guid AccountId) : IRequest;
