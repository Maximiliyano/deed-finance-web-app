using Deed.Domain.Constants;
using Deed.Domain.Enums;
using FluentValidation;

namespace Deed.Application.BudgetEstimations.Commands.Update;

internal sealed class UpdateBudgetEstimationCommandValidator : AbstractValidator<UpdateBudgetEstimationCommand>
{
    public UpdateBudgetEstimationCommandValidator()
    {
        RuleFor(c => c.Description)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.BudgetAmount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.BudgetCurrency)
            .Must(c => c != CurrencyType.None)
            .WithMessage("Budget currency must be set.");
    }
}
