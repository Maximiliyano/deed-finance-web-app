using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Errors;
using FluentValidation;

namespace Deed.Application.Transfers.Commands.Create;

internal sealed class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(c => c.SourceCapitalId)
            .GreaterThan(0);

        RuleFor(c => c.DestinationCapitalId)
            .GreaterThan(0);

        RuleFor(c => c.Amount)
            .GreaterThan(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero);

        RuleFor(c => c.DestinationAmount)
            .GreaterThan(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero);
    }
}
