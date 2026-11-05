using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using FluentValidation;

namespace Deed.Application.UtilityBills.Commands.Create;

internal sealed class CreateUtilityBillCommandValidator : AbstractValidator<CreateUtilityBillCommand>
{
    public CreateUtilityBillCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.EstimatedAmount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.Currency)
            .Must(c => Enum.TryParse<CurrencyType>(c, out var pc) && !pc.Equals(CurrencyType.None))
            .WithError(ValidationErrors.General.InvalidCurrencyType);

        RuleFor(c => c.DueDayOfMonth)
            .InclusiveBetween(1, 31);

        RuleFor(c => c.CapitalId)
            .GreaterThan(0);

        RuleFor(c => c.CategoryId)
            .GreaterThan(0);
    }
}
