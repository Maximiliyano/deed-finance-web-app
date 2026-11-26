using FluentValidation;

namespace Deed.Application.BankConnections.Commands.ConnectPrivatCard;

internal sealed class ConnectPrivatCardCommandValidator : AbstractValidator<ConnectPrivatCardCommand>
{
    public ConnectPrivatCardCommandValidator()
    {
        RuleFor(c => c.MerchantId)
            .NotEmpty()
            .MaximumLength(32)
            .Must(id => id.Trim().All(char.IsAsciiDigit))
            .WithMessage("The merchant id must contain digits only.");

        RuleFor(c => c.Password)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.CardNumber)
            .NotEmpty()
            .Must(card => BankConnectionExtensions.IsValidCardNumber(BankConnectionExtensions.NormalizeCardNumber(card)))
            .WithMessage("Enter a valid 16-digit card number.");
    }
}
