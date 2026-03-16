using Deed.Application.Categories.Queries.GetAll;
using Deed.Domain.Enums;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.Categories.Queries;

public sealed class GetAllCategoryQueryValidatorTests
{
    private readonly GetAllCategoryQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ExpensesType_PassesValidation()
    {
        var query = new GetAllCategoryQuery(CategoryType.Expenses);
        var result = await _validator.TestValidateAsync(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_IncomesType_PassesValidation()
    {
        var query = new GetAllCategoryQuery(CategoryType.Incomes);
        var result = await _validator.TestValidateAsync(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NoneType_FailsValidation()
    {
        var query = new GetAllCategoryQuery(CategoryType.None);
        var result = await _validator.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(q => q.Type);
    }

    [Fact]
    public async Task Validate_NullType_PassesValidation()
    {
        var query = new GetAllCategoryQuery(null);
        var result = await _validator.TestValidateAsync(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
