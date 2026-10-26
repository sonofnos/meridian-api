using Meridian.Application.Common.Interfaces;

namespace Meridian.Infrastructure.Persistence;

public sealed class UnitOfWork(MeridianDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
