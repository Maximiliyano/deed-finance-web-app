using Deed.Application.Categories.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using Deed.Domain.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace Deed.Application.Categories;

internal static class CategoryRuleBuilderExtensions
{
    public static IRuleBuilderOptionsConditions<T, int> MustBeCategoryOfType<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        ICategoryRepository repository,
        CategoryType type) =>
        ruleBuilder.CustomAsync((categoryId, context, ct) => ValidateAsync(categoryId, context, repository, type, ct));

    public static IRuleBuilderOptionsConditions<T, int?> MustBeCategoryOfType<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        ICategoryRepository repository,
        CategoryType type) =>
        ruleBuilder.CustomAsync((categoryId, context, ct) => ValidateAsync(categoryId!.Value, context, repository, type, ct));

    private static async Task ValidateAsync<T>(
        int categoryId,
        ValidationContext<T> context,
        ICategoryRepository repository,
        CategoryType type,
        CancellationToken cancellationToken)
    {
        var category = await repository.GetAsync(new CategoryByIdSpecification(categoryId), cancellationToken).ConfigureAwait(false);

        if (category is null)
        {
            AddFailure(context, ValidationErrors.General.NotFound("category"));
        }
        else if (category.Type != type)
        {
            AddFailure(context, ValidationErrors.Category.InvalidType);
        }
    }

    private static void AddFailure<T>(ValidationContext<T> context, Error error) =>
        context.AddFailure(new ValidationFailure(context.PropertyPath, error.Message) { ErrorCode = error.Code });
}
