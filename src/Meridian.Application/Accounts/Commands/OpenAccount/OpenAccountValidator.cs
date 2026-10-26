using FluentValidation;

namespace Meridian.Application.Accounts.Commands.OpenAccount;

public sealed class OpenAccountValidator : AbstractValidator<OpenAccountCommand>
{
    private static readonly HashSet<string> SupportedCurrencies = ["KES", "USD", "GBP", "EUR", "NGN"];

    public OpenAccountValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty()
            .Must(c => SupportedCurrencies.Contains(c.ToUpperInvariant()))
            .WithMessage("Currency must be one of: KES, USD, GBP, EUR, NGN.");
    }
}
