using Deed.Domain.Constants;
using FluentValidation;

namespace Deed.Application.UtilityBills.Commands.Pay;

internal sealed class PayUtilityBillCommandValidator : AbstractValidator<PayUtilityBillCommand>
{
    public PayUtilityBillCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0);

        RuleFor(c => c.Amount)
            .GreaterThan(ValidationConstants.ZeroValue);
    }
}
