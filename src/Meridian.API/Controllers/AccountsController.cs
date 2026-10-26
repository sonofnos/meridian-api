using MediatR;
using Meridian.Application.Accounts.Commands.FreezeAccount;
using Meridian.Application.Accounts.Commands.OpenAccount;
using Meridian.Application.Accounts.Queries.GetAccount;
using Meridian.Application.Transactions.Queries.GetTransactionHistory;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.API.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<OpenAccountResult>(StatusCodes.Status201Created)]
    public async Task<IActionResult> OpenAccount([FromBody] OpenAccountCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetAccount), new { id = result.AccountId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAccountQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/freeze")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Freeze(Guid id, CancellationToken ct)
    {
        await mediator.Send(new FreezeAccountCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unfreeze")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Unfreeze(Guid id, CancellationToken ct)
    {
        await mediator.Send(new UnfreezeAccountCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/transactions")]
    [ProducesResponseType<IReadOnlyList<TransactionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTransactionHistoryQuery(id, page, pageSize), ct);
        return Ok(result);
    }
}
