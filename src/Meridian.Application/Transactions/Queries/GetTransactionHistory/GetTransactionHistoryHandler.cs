using MediatR;
using Meridian.Domain.Transactions;

namespace Meridian.Application.Transactions.Queries.GetTransactionHistory;

public sealed class GetTransactionHistoryHandler(ITransactionRepository transactions)
    : IRequestHandler<GetTransactionHistoryQuery, IReadOnlyList<TransactionDto>>
{
    public async Task<IReadOnlyList<TransactionDto>> Handle(GetTransactionHistoryQuery query, CancellationToken ct)
    {
        var txs = await transactions.GetByAccountIdAsync(query.AccountId, query.Page, query.PageSize, ct);
        return txs.Select(t => new TransactionDto(
            t.Id, t.DebitAccountId, t.CreditAccountId,
            t.Amount, t.Currency, t.Reference, t.Description,
            t.Status, t.CreatedAt, t.CompletedAt)).ToList();
    }
}
