using Deed.Domain.Constants;
using Deed.Domain.Enums;
using FluentValidation;

namespace Deed.Application.Goals.Commands.Update;

internal sealed class UpdateGoalCommandValidator : AbstractValidator<UpdateGoalCommand>
{
    public UpdateGoalCommandValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.TargetAmount)
            .GreaterThan(ValidationConstants.ZeroValue);

        RuleFor(c => c.Currency)
            .Must(c => c != CurrencyType.None)
            .WithMessage("Currency must be set.");

        RuleFor(c => c.CurrentAmount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.Note)
            .MaximumLength(512);
    }
}
