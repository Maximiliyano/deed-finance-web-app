using Deed.Application.Abstractions;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.UtilityBills.Commands.Update;

internal sealed class UpdateUtilityBillCommandValidator : AbstractValidator<UpdateUtilityBillCommand>
{
    public UpdateUtilityBillCommandValidator(
        ICapitalRepository capitalRepository,
        ICategoryRepository categoryRepository,
        IUser user)
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
            .GreaterThan(0)
            .MustAsync(async (capitalId, ct) => await capitalRepository
                .AnyAsync(new CapitalByIdSpecification(capitalId, user.Name), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("capital"));

        RuleFor(c => c.CategoryId)
            .GreaterThan(0)
            .MustAsync(async (categoryId, ct) => await categoryRepository
                .AnyAsync(new CategoryByIdSpecification(categoryId), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("category"))
            .MustAsync(async (categoryId, ct) => (await categoryRepository
                .GetAsync(new CategoryByIdSpecification(categoryId), ct).ConfigureAwait(false))?.Type == CategoryType.Expenses)
            .WithError(ValidationErrors.Category.InvalidType);
    }
}
