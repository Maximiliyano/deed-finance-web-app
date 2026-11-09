using Deed.Application.Abstractions.Caching;
using Deed.Application.Categories.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Commands;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repositoryMock = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cacheMock = Substitute.For<ICacheService>();

    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_repositoryMock, _unitOfWorkMock, _cacheMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoryId_WhenCategoryIsCreatedSuccessfully()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            "New Category",
            CategoryType.Expenses,
            100.0m,
            TimePeriodType.Day);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Create(Arg.Is<Category>(c =>
            c.Name == command.Name &&
            c.Type == command.Type &&
            c.PlannedPeriodAmount.Equals(command.PlannedPeriodAmount) &&
            c.Period == command.Period));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheMock.Received(CacheKeys.AllCategoryKeys().Count()).RemoveAsync(Arg.Is<string>(k => k.StartsWith(CacheKeys.Categories)), Arg.Any<CancellationToken>());
    }
}
