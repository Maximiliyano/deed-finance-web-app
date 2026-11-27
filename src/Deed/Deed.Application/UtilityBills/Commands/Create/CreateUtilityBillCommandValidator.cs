using Deed.Application.Abstractions;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Categories;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.UtilityBills.Commands.Create;

internal sealed class CreateUtilityBillCommandValidator : AbstractValidator<CreateUtilityBillCommand>
{
    public CreateUtilityBillCommandValidator(
        ICapitalRepository capitalRepository,
        ICategoryRepository categoryRepository,
        IUser user)
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.EstimatedAmount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.UnitPrice)
            .GreaterThan(ValidationConstants.ZeroValue)
            .LessThan(ValidationConstants.MaxUnitPrice)
            .When(c => c.UnitPrice is not null);

        RuleFor(c => c.Unit)
            .MaximumLength(ValidationConstants.MaxLengthUnit);

        RuleFor(c => c.Currency)
            .Must(c => Enum.TryParse<CurrencyType>(c, out var pc) && !pc.Equals(CurrencyType.None))
            .WithError(ValidationErrors.General.InvalidCurrencyType);

        RuleFor(c => c.DueDayOfMonth)
            .InclusiveBetween(1, 31);

        RuleFor(c => c.CapitalId)
            .GreaterThan(0)
            .MustAsync(async (capitalId, ct) => await capitalRepository
                .AnyAsync(new CapitalByIdSpecification(capitalId, user.Id), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("capital"));

        RuleFor(c => c.CategoryId)
            .GreaterThan(0)
            .MustBeCategoryOfType(categoryRepository, CategoryType.Expenses);
    }
}
