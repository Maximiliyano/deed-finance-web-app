using FluentValidation;

namespace Deed.Application.BankConnections.Commands.ConnectPrivat;

internal sealed class ConnectPrivatCommandValidator : AbstractValidator<ConnectPrivatCommand>
{
    public ConnectPrivatCommandValidator()
    {
        RuleFor(c => c.Token)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(c => c.Label)
            .MaximumLength(64);

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
