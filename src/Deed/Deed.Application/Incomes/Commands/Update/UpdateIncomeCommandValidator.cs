using Deed.Application.Categories.Specifications;
using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Incomes.Commands.Update;

internal sealed class UpdateIncomeCommandValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(e => e.Amount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero)
            .When(e => e.Amount.HasValue);

        RuleFor(e => e.Purpose)
            .MaximumLength(ValidationConstants.MaxLengthPurpose)
            .When(e => e.Purpose is not null);

        RuleForEach(e => e.TagNames)
            .MaximumLength(ValidationConstants.MaxLengthTagName)
            .When(e => e.TagNames is not null);

        RuleFor(e => e.CategoryId)
            .MustAsync(async (categoryId, ct) => await categoryRepository
                .AnyAsync(new CategoryByIdSpecification(categoryId!.Value), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("category"))
            .MustAsync(async (categoryId, ct) => (await categoryRepository
                .GetAsync(new CategoryByIdSpecification(categoryId!.Value), ct).ConfigureAwait(false))?.Type == CategoryType.Incomes)
            .WithError(ValidationErrors.Category.InvalidType)
            .When(e => e.CategoryId.HasValue);
    }
}
