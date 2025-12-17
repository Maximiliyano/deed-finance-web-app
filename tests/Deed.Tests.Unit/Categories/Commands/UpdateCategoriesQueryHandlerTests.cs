using Deed.Application.Capitals.Commands.Update;
using Deed.Application.Categories.Commands.UpdateRange;
using Deed.Application.Categories.Requests;
using Deed.Application.Categories.Response;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Commands;

public sealed class UpdateCategoriesQueryHandlerTests
{
    private readonly ICategoryRepository _repositoryMock = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly UpdateCategoriesCommandHandler _handler;

    public UpdateCategoriesQueryHandlerTests()
    {
        _handler = new UpdateCategoriesCommandHandler(_repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCategoriesNotFound()
    {
        // Arrange
        var command = new UpdateCategoriesCommand([
            new(1, null, null, null, null)
        ]);

        _repositoryMock
            .GetAllAsync(Arg.Any<CategoriesByQuerySpecification>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.General.NotFound("category"));

        await _repositoryMock.Received(1).GetAllAsync(Arg.Is<CategoriesByQuerySpecification>(spec => spec.Tracking == true));
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCategoryHaveNothingToUpdate()
    {
        // Arrange
        const int id = 1;

        var category = new Category(id) { Name = "Test", Type = CategoryType.Expenses };
        var command = new UpdateCategoriesCommand([
            new(id, category.Name, category.Type, category.PlannedPeriodAmount, category.Period)
        ]);
        _repositoryMock
            .GetAllAsync(Arg.Any<CategoriesByQuerySpecification>())
            .Returns([category]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<CategoriesByQuerySpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("New Category", CategoryType.Expenses, 1.0, PerPeriodType.Daily)]
    [InlineData(null, CategoryType.Incomes, 1.0, PerPeriodType.Weekly)]
    [InlineData("New Category", null, 1.0, PerPeriodType.Daily)]
    [InlineData("New Category", CategoryType.Expenses, null, PerPeriodType.Monthly)]
    [InlineData("New Category", CategoryType.Expenses, null, PerPeriodType.Yearly)]
    [InlineData("New Category", CategoryType.Incomes, 1.0, null)]
    [InlineData(null, null, null, null)]
    [InlineData("", CategoryType.Incomes, 0.0, PerPeriodType.Weekly)]
    [InlineData("New Category", CategoryType.Expenses, -10.0, PerPeriodType.Monthly)]
    [InlineData("New Category", CategoryType.Expenses, -10.0, PerPeriodType.Yearly)]

    public async Task Handle_ShouldUpdateCategorySuccessfully(
        string? name,
        CategoryType? type,
        double? plannedPeriodAmount,
        PerPeriodType? perPeriodType)
    {
        // Arrange
        const int id = 1;
        const string oldName = "Old Category";
        const CategoryType oldType = CategoryType.Expenses;
        const decimal oldPlannedPeriodAmount = 1.2m;
        const PerPeriodType oldPerPeriodType = PerPeriodType.Daily;

        var category = new Category(id)
        {
            Name = oldName,
            Type = oldType,
            PlannedPeriodAmount = oldPlannedPeriodAmount,
            Period = oldPerPeriodType
        };

        decimal? newPlannedPeriodAmount = plannedPeriodAmount is null ? null : (decimal)plannedPeriodAmount;
        var command = new UpdateCategoriesCommand([
            new(id, name, type, newPlannedPeriodAmount, perPeriodType)
        ]);

        _repositoryMock
            .GetAllAsync(Arg.Any<CategoriesByQuerySpecification>())
            .Returns([category]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        category.Name.Should().Be(name ?? oldName);
        category.Type.Should().Be(type ?? oldType);
        category.Period.Should().Be(perPeriodType ?? oldPerPeriodType);
        category.PlannedPeriodAmount.Should().Be(newPlannedPeriodAmount ?? oldPlannedPeriodAmount);

        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<CategoriesByQuerySpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
