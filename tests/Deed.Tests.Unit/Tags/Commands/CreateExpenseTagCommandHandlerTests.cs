using Deed.Application.Auth;
using Deed.Application.Tags.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Tags.Commands;

public sealed class CreateExpenseTagCommandHandlerTests
{
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly CreateExpenseTagCommandHandler _handler;

    public CreateExpenseTagCommandHandlerTests()
    {
        _user.Name.Returns("testuser");
        _handler = new CreateExpenseTagCommandHandler(_expenseRepository, _unitOfWork, _user);
    }

    [Fact]
    public async Task Handle_WhenExpenseExists_ReturnsSuccess()
    {
        // Arrange
        Expense expense = new() { Amount = 100m, PaymentDate = DateTimeOffset.UtcNow, CategoryId = 1, CapitalId = 1 };
        _expenseRepository.GetAsync(Arg.Any<ISpecification<Expense>>(), Arg.Any<CancellationToken>()).Returns(expense);
        CreateExpenseTagCommand command = new(1, "food");

        // Act
        Result<int> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExpenseNotFound_ReturnsFailure()
    {
        // Arrange
        _expenseRepository.GetAsync(Arg.Any<ISpecification<Expense>>(), Arg.Any<CancellationToken>())
            .Returns((Expense?)null);
        CreateExpenseTagCommand command = new(99, "food");

        // Act
        Result<int> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
