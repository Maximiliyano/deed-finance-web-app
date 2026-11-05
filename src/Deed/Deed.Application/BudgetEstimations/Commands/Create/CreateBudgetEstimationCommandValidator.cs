using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using FluentValidation;

namespace Deed.Application.BudgetEstimations.Commands.Create;

internal sealed class CreateBudgetEstimationCommandValidator : AbstractValidator<CreateBudgetEstimationCommand>
{
    public CreateBudgetEstimationCommandValidator()
    {
        RuleFor(c => c.Description)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.BudgetAmount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.BudgetCurrency)
            .Must(c => Enum.TryParse<CurrencyType>(c, out var pc) && !pc.Equals(CurrencyType.None))
            .WithError(DomainErrors.General.InvalidCurrency);
    }
}
