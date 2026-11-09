using Deed.Application.Auth;
using Deed.Application.Expenses.Commands.Update;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Expenses.Commands;

public sealed class UpdateExpenseCommandValidatorTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IDateTimeProvider _provider = Substitute.For<IDateTimeProvider>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly UpdateExpenseCommandValidator _validator;

    public UpdateExpenseCommandValidatorTests()
    {
        _user.Name.Returns("testuser");
        _provider.UtcNow.Returns(DateTimeOffset.UtcNow);
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Food", Type = CategoryType.Expenses });
        _validator = new UpdateExpenseCommandValidator(_categoryRepository, _capitalRepository, _provider, _user);
    }

    [Fact]
    public async Task Validate_ExistingCategory_PassesValidation()
    {
        // Act
        var result = await _validator.TestValidateAsync(new UpdateExpenseCommand(1, CategoryId: 2));

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Validate_CategoryNotFound_FailsValidation()
    {
        // Arrange
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        // Act
        var result = await _validator.TestValidateAsync(new UpdateExpenseCommand(1, CategoryId: 2));

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Validate_CategoryCheckDoesNotDependOnExpenseIds()
    {
        // Act
        await _validator.TestValidateAsync(new UpdateExpenseCommand(1, CategoryId: 1));

        // Assert
        await _categoryRepository.Received(1).GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>());
    }
}
