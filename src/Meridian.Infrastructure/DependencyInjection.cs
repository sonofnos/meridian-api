using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Accounts;
using Meridian.Domain.Transactions;
using Meridian.Infrastructure.Outbox;
using Meridian.Infrastructure.Persistence;
using Meridian.Infrastructure.Repositories;
using Meridian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<MeridianDbContext>(opts =>
            opts.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IAccountNumberGenerator, AccountNumberGenerator>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeridianDbContext>();
        await db.Database.MigrateAsync();
    }
}
