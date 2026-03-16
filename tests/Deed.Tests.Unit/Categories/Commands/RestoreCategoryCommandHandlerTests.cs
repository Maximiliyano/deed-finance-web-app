using Deed.Application.Categories.Commands.Restore;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Commands;

public sealed class RestoreCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repositoryMock = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly RestoreCategoryCommandHandler _handler;

    public RestoreCategoryCommandHandlerTests()
    {
        _handler = new RestoreCategoryCommandHandler(_repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_RestoresAndReturnsResponse()
    {
        // Arrange
        var category = new Category(1) { Name = "Groceries", Type = CategoryType.Expenses, IsDeleted = true };
        var command = new RestoreCategoryCommand(1);

        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>()).Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Groceries");
        category.IsDeleted.Should().BeFalse();

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new RestoreCategoryCommand(99);
        _repositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>()).Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("category"));
        await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
