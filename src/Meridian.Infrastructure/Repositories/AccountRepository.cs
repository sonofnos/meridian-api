using Meridian.Domain.Accounts;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Repositories;

public sealed class AccountRepository(MeridianDbContext db) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct) =>
        db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);

    public async Task<IReadOnlyList<Account>> GetByOwnerIdAsync(string ownerId, CancellationToken ct) =>
        await db.Accounts.Where(a => a.OwnerId == ownerId).ToListAsync(ct);

    public async Task AddAsync(Account account, CancellationToken ct) =>
        await db.Accounts.AddAsync(account, ct);

    public void Update(Account account) => db.Accounts.Update(account);
}
