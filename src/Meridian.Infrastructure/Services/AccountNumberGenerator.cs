using Meridian.Application.Common.Interfaces;

namespace Meridian.Infrastructure.Services;

public sealed class AccountNumberGenerator : IAccountNumberGenerator
{
    private static readonly Random _rng = new();

    public string Generate()
    {
        // Format: MRD + 10 digits
        return "MRD" + string.Concat(Enumerable.Range(0, 10).Select(_ => _rng.Next(0, 10)));
    }
}
