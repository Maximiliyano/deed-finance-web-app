using Deed.Application.Categories.Specifications;
using Deed.Application.Abstractions;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Incomes.Commands.Update;

internal sealed class UpdateIncomeCommandValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(e => e.CategoryId)
            .MustAsync(async (categoryId, _) => await categoryRepository
                .AnyAsync(new CategoryByIdSpecification(categoryId!.Value)).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("category"))
            .MustAsync(async (categoryId, _) => (await categoryRepository
                .GetAsync(new CategoryByIdSpecification(categoryId!.Value)).ConfigureAwait(false))?.Type == CategoryType.Expenses)
            .WithError(ValidationErrors.Category.InvalidType)
            .When(e => e.CategoryId.HasValue);
    }
}
