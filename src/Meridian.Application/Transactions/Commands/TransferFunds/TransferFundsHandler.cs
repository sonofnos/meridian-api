using MediatR;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Accounts;
using Meridian.Domain.Exceptions;
using Meridian.Domain.Transactions;

namespace Meridian.Application.Transactions.Commands.TransferFunds;

public sealed class TransferFundsHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IUnitOfWork uow) : IRequestHandler<TransferFundsCommand, TransferFundsResult>
{
    public async Task<TransferFundsResult> Handle(TransferFundsCommand cmd, CancellationToken ct)
    {
        // Idempotency: replay returns existing result
        var existing = await transactions.GetByReferenceAsync(cmd.Reference, ct);
        if (existing is not null)
        {
            if (existing.Status == TransactionStatus.Completed)
                return new TransferFundsResult(existing.Id, existing.Reference, existing.Amount, existing.Currency);
            throw new DuplicateReferenceException(cmd.Reference);
        }

        var debit = await accounts.GetByIdAsync(cmd.DebitAccountId, ct)
            ?? throw new AccountNotFoundException(cmd.DebitAccountId);

        var credit = await accounts.GetByIdAsync(cmd.CreditAccountId, ct)
            ?? throw new AccountNotFoundException(cmd.CreditAccountId);

        var tx = Transaction.Initiate(
            cmd.DebitAccountId, cmd.CreditAccountId,
            cmd.Amount, cmd.Currency, cmd.Reference, cmd.Description);

        try
        {
            // Debit first, then credit — both under the same unit of work
            debit.Debit(cmd.Amount, cmd.Reference);
            credit.Credit(cmd.Amount, cmd.Reference);
            tx.Complete();
        }
        catch (DomainException ex)
        {
            tx.Fail(ex.Message);
            await transactions.AddAsync(tx, ct);
            await uow.SaveChangesAsync(ct);
            throw;
        }

        accounts.Update(debit);
        accounts.Update(credit);
        await transactions.AddAsync(tx, ct);
        await uow.SaveChangesAsync(ct);

        return new TransferFundsResult(tx.Id, tx.Reference, tx.Amount, tx.Currency);
    }
}
