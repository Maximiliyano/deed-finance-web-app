using FluentValidation;

namespace Deed.Application.BankConnections.Commands.ConnectPrivatMail;

internal sealed class ConnectPrivatMailCommandValidator : AbstractValidator<ConnectPrivatMailCommand>
{
    public ConnectPrivatMailCommandValidator()
    {
        RuleFor(c => c.Label)
            .MaximumLength(64);

        RuleFor(c => c.Currency)
            .Matches("^[A-Za-z]{3}$")
            .When(c => !string.IsNullOrWhiteSpace(c.Currency));

        RuleFor(c => c.Iban)
            .NotEmpty()
            .Must(iban => IsValidIban(BankConnectionExtensions.NormalizeIban(iban)))
            .WithMessage("The IBAN must look like UA followed by 27 characters.");
    }

    private static bool IsValidIban(string iban)
    {
        return iban.Length == 29
            && iban.StartsWith("UA", StringComparison.Ordinal)
            && iban[2..].All(char.IsAsciiLetterOrDigit);
    }
}
