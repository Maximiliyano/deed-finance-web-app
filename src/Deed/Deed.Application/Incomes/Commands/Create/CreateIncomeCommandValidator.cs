using Deed.Application.Categories.Specifications;
using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Incomes.Commands.Create;

internal sealed class CreateIncomeCommandValidator : AbstractValidator<CreateIncomeCommand>
{
    public CreateIncomeCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(e => e.CategoryId)
            .MustAsync(async (categoryId, ct) => await categoryRepository
                .AnyAsync(new CategoryByIdSpecification(categoryId), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("category"))
            .MustAsync(async (categoryId, ct) => (await categoryRepository
                .GetAsync(new CategoryByIdSpecification(categoryId), ct).ConfigureAwait(false))?.Type == CategoryType.Incomes)
            .WithError(ValidationErrors.Category.InvalidType);

        RuleFor(i => i.Amount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero);
    }
}
