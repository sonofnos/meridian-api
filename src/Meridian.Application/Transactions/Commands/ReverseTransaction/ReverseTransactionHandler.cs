using MediatR;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Accounts;
using Meridian.Domain.Exceptions;
using Meridian.Domain.Transactions;

namespace Meridian.Application.Transactions.Commands.ReverseTransaction;

public sealed class ReverseTransactionHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IUnitOfWork uow) : IRequestHandler<ReverseTransactionCommand>
{
    public async Task Handle(ReverseTransactionCommand cmd, CancellationToken ct)
    {
        var tx = await transactions.GetByIdAsync(cmd.TransactionId, ct)
            ?? throw new TransactionNotFoundException(cmd.TransactionId);

        var debit = await accounts.GetByIdAsync(tx.DebitAccountId, ct)
            ?? throw new AccountNotFoundException(tx.DebitAccountId);

        var credit = await accounts.GetByIdAsync(tx.CreditAccountId, ct)
            ?? throw new AccountNotFoundException(tx.CreditAccountId);

        // Reversal: credit back the debited account, debit the credited account
        var reversalRef = $"REV-{tx.Reference}";
        credit.Debit(tx.Amount, reversalRef);
        debit.Credit(tx.Amount, reversalRef);
        tx.Reverse();

        accounts.Update(debit);
        accounts.Update(credit);
        transactions.Update(tx);
        await uow.SaveChangesAsync(ct);
    }
}
