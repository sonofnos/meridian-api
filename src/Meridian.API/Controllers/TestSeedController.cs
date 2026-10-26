using MediatR;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.API.Controllers;

// Test-only controller: disabled in production via environment check
[ApiController]
[Route("api/test")]
public class TestSeedController(IAccountRepository accounts, IUnitOfWork uow, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("seed-account")]
    public async Task<IActionResult> SeedAccount([FromBody] SeedAccountRequest req, CancellationToken ct)
    {
        if (!env.IsEnvironment("Test"))
            return NotFound();

        var account = await accounts.GetByIdAsync(req.AccountId, ct);
        if (account is null) return NotFound();

        account.Credit(req.Amount, $"SEED-{Guid.NewGuid()}");
        accounts.Update(account);
        await uow.SaveChangesAsync(ct);

        return NoContent();
    }
}

public sealed record SeedAccountRequest(Guid AccountId, decimal Amount);
