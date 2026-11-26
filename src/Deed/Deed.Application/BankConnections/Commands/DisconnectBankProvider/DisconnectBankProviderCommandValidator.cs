using Deed.Domain.Enums;
using FluentValidation;

namespace Deed.Application.BankConnections.Commands.DisconnectBankProvider;

internal sealed class DisconnectBankProviderCommandValidator : AbstractValidator<DisconnectBankProviderCommand>
{
    public DisconnectBankProviderCommandValidator()
    {
        RuleFor(c => c.Provider)
            .Must(provider => Enum.TryParse<BankProvider>(provider, true, out var parsed) && parsed != BankProvider.None)
            .WithMessage("Unknown bank.");
    }
}
