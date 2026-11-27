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
            .GreaterThan(ValidationConstants.ZeroValue)
            .When(c => c.MeterReading is null);

        RuleFor(c => c.MeterReading)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
            .LessThan(ValidationConstants.MaxMeterReading)
            .When(c => c.MeterReading is not null);
    }
}
