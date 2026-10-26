using MediatR;

namespace Meridian.Application.Transactions.Commands.TransferFunds;

public sealed record TransferFundsCommand(
    Guid DebitAccountId,
    Guid CreditAccountId,
    decimal Amount,
    string Currency,
    string Reference,
    string? Description = null) : IRequest<TransferFundsResult>;

public sealed record TransferFundsResult(Guid TransactionId, string Reference, decimal Amount, string Currency);
