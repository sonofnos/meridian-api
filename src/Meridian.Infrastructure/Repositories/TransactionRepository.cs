using Meridian.Domain.Transactions;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Repositories;

public sealed class TransactionRepository(MeridianDbContext db) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Transaction?> GetByReferenceAsync(string reference, CancellationToken ct) =>
        db.Transactions.FirstOrDefaultAsync(t => t.Reference == reference, ct);

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, int page, int pageSize, CancellationToken ct) =>
        await db.Transactions
            .Where(t => t.DebitAccountId == accountId || t.CreditAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task AddAsync(Transaction transaction, CancellationToken ct) =>
        await db.Transactions.AddAsync(transaction, ct);

    public void Update(Transaction transaction) => db.Transactions.Update(transaction);
}
