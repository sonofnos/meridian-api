using MediatR;
using Meridian.Application.Transactions.Commands.ReverseTransaction;
using Meridian.Application.Transactions.Commands.TransferFunds;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.API.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpPost("transfer")]
    [ProducesResponseType<TransferFundsResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Transfer([FromBody] TransferFundsCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reverse")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reverse(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ReverseTransactionCommand(id), ct);
        return NoContent();
    }
}
