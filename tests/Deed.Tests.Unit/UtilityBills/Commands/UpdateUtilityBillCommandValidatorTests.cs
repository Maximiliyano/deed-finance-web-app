using Deed.Application.Auth;
using Deed.Application.UtilityBills.Commands.Update;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class UpdateUtilityBillCommandValidatorTests
{
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly UpdateUtilityBillCommandValidator _validator;

    public UpdateUtilityBillCommandValidatorTests()
    {
        _user.Name.Returns("testuser");
        _capitalRepository.AnyAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(true);
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Utilities", Type = CategoryType.Expenses });
        _validator = new UpdateUtilityBillCommandValidator(_capitalRepository, _categoryRepository, _user);
    }

    private static UpdateUtilityBillCommand ValidCommand => new(
        1, "Electric", 500m, nameof(CurrencyType.UAH), 15, 1, 7, true);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_CapitalNotOwnedOrMissing_FailsValidation()
    {
        _capitalRepository.AnyAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(false);
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.CapitalId);
    }

    [Fact]
    public async Task Validate_CategoryMissing_FailsValidation()
    {
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns((Category?)null);
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Validate_IncomeCategory_FailsValidation()
    {
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Salary", Type = CategoryType.Incomes });
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }
}
