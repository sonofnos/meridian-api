using Meridian.Domain.Accounts;
using Meridian.Domain.Common;
using Meridian.Domain.Transactions;
using Meridian.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Persistence;

public class MeridianDbContext(DbContextOptions<MeridianDbContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Ignore<DomainEvent>();
        builder.ApplyConfigurationsFromAssembly(typeof(MeridianDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await DispatchDomainEventsToOutboxAsync();
        return await base.SaveChangesAsync(ct);
    }

    private async Task DispatchDomainEventsToOutboxAsync()
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var outboxMessages = aggregates
            .SelectMany(a => a.DomainEvents)
            .Select(e => OutboxMessage.From(e))
            .ToList();

        foreach (var agg in aggregates) agg.ClearDomainEvents();

        await OutboxMessages.AddRangeAsync(outboxMessages);
    }
}
