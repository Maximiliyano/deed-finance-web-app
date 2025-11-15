using Deed.Application.Categories.Commands.Update;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Commands;

public sealed class UpdateCategoryQueryHandlerTests
{
    private readonly ICategoryRepository _repositoryMock = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryQueryHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(_repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var command = new UpdateCategoryCommand(1);

        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>()).Returns((Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("category"));

        await _repositoryMock.Received(1).GetAsync(Arg.Any<CategoryByIdSpecification>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

        _repositoryMock.DidNotReceive().Update(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCategoryHaveNothingToUpdate()
    {
        // Arrange
        const int id = 1;

        var category = new Category(id) { Name = "Test", Type = CategoryType.Expenses };
        var command = new UpdateCategoryCommand(id);

        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>()).Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _repositoryMock.Received(1).GetAsync(Arg.Any<CategoryByIdSpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        _repositoryMock.Received(1).Update(category);
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
        var command = new UpdateCategoryCommand(id, name, newPlannedPeriodAmount, perPeriodType, type);

        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>()).Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        category.Name.Should().Be(name ?? oldName);
        category.Type.Should().Be(type ?? oldType);
        category.Period.Should().Be(perPeriodType ?? oldPerPeriodType);
        category.PlannedPeriodAmount.Should().Be(newPlannedPeriodAmount ?? oldPlannedPeriodAmount);

        await _repositoryMock.Received(1).GetAsync(Arg.Any<CategoryByIdSpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        _repositoryMock.Received(1).Update(category);
    }
}
