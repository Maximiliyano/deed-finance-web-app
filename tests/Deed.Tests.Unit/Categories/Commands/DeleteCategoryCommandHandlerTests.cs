using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Data;
using Deed.Application.Categories.Commands.Delete;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Commands;

public sealed class DeleteCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repositoryMock = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cacheMock = Substitute.For<ICacheService>();

    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_repositoryMock, _unitOfWorkMock, _cacheMock);
    }

    [Theory]
    [InlineData(CategoryType.Incomes, "IncomesWithHighAmountOfDollars", 0, TimePeriodType.None)]
    [InlineData(CategoryType.Expenses, "E", 100, TimePeriodType.Day)]
    public async Task Handle_ShouldDeleteCategory_ReturnSuccess(
        CategoryType type,
        string name,
        decimal periodAmount,
        TimePeriodType periodType)
    {
        // Arrange
        var category = new Category(1)
        {
            Name = name,
            Type = type,
            PlannedPeriodAmount = periodAmount,
            Period = periodType
        };
        var command = new DeleteCategoryCommand(1);

        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Delete(category);

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheMock.Received(CacheKeys.AllCategoryKeys().Count()).RemoveAsync(Arg.Is<string>(k => k.StartsWith(CacheKeys.Categories)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryHasReferences_ReturnsConflict()
    {
        // Arrange
        var category = new Category(1) { Name = "Food", Type = CategoryType.Expenses };

        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>()).Returns(category);
        _repositoryMock.HasReferencesAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.Handle(new DeleteCategoryCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.Category.ReferenceExists);

        _repositoryMock.DidNotReceive().Delete(Arg.Any<Category>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheMock.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteNotFoundCategory_ReturnFailure()
    {
        // Arrange
        var command = new DeleteCategoryCommand(1);

        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>())
            .Returns((Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.General.NotFound("category"));

        _repositoryMock.Received(0).Delete(Arg.Any<Category>());

        await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheMock.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
