using MediatR;
using Meridian.Domain.Transactions;

namespace Meridian.Application.Transactions.Queries.GetTransactionHistory;

public sealed record GetTransactionHistoryQuery(
    Guid AccountId,
    int Page = 1,
    int PageSize = 20) : IRequest<IReadOnlyList<TransactionDto>>;

public sealed record TransactionDto(
    Guid Id,
    Guid DebitAccountId,
    Guid CreditAccountId,
    decimal Amount,
    string Currency,
    string Reference,
    string? Description,
    TransactionStatus Status,
    DateTime CreatedAt,
    DateTime? CompletedAt);
