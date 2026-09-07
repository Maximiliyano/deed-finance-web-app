using Deed.Domain.Constants;
using Deed.Domain.Enums;
using FluentValidation;

namespace Deed.Application.UtilityBills.Commands.Update;

internal sealed class UpdateUtilityBillCommandValidator : AbstractValidator<UpdateUtilityBillCommand>
{
    public UpdateUtilityBillCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.EstimatedAmount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.Currency)
            .Must(c => c != CurrencyType.None)
            .WithMessage("Currency must be set.");

        RuleFor(c => c.DueDayOfMonth)
            .InclusiveBetween(1, 31);

        RuleFor(c => c.CapitalId)
            .GreaterThan(0);

        RuleFor(c => c.CategoryId)
            .GreaterThan(0);
    }
}
