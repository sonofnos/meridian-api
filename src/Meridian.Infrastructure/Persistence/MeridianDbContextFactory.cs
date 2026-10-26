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

        return new MeridianDbContext(opts);
    }
}
