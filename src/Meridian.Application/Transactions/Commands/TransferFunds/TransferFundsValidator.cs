using FluentValidation;

namespace Meridian.Application.Transactions.Commands.TransferFunds;

public sealed class TransferFundsValidator : AbstractValidator<TransferFundsCommand>
{
    public TransferFundsValidator()
    {
        RuleFor(x => x.DebitAccountId).NotEmpty();
        RuleFor(x => x.CreditAccountId).NotEmpty()
            .NotEqual(x => x.DebitAccountId).WithMessage("Debit and credit accounts must differ.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(100);
    }
}
