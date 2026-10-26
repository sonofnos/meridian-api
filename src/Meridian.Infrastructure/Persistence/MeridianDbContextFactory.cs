using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Meridian.Infrastructure.Persistence;

public class MeridianDbContextFactory : IDesignTimeDbContextFactory<MeridianDbContext>
{
    public MeridianDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<MeridianDbContext>()
            .UseNpgsql("Host=localhost;Database=meridian;Username=postgres;Password=postgres")
            .Options;

        return new MeridianDbContext(opts, new NoOpPublisher());
    }

    private sealed class NoOpPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;
    }
}
