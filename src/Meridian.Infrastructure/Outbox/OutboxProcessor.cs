using MediatR;
using Meridian.Domain.Common;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Meridian.Infrastructure.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingMessagesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeridianDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        foreach (var msg in pending)
        {
            try
            {
                var type = Type.GetType(msg.EventType);
                if (type is null)
                {
                    msg.MarkFailed($"Unknown type: {msg.EventType}");
                    continue;
                }

                var domainEvent = (DomainEvent)JsonSerializer.Deserialize(msg.Payload, type)!;
                await publisher.Publish(domainEvent, ct);
                msg.MarkProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {MessageId}", msg.Id);
                msg.MarkFailed(ex.Message);
            }
        }

        if (pending.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
