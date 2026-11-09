using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Commands.Update;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Debts.Commands;

public sealed class UpdateDebtCommandHandlerTests
{
    private readonly IDebtRepository _repositoryMock = Substitute.For<IDebtRepository>();
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpdateDebtCommandHandler _handler;

    public UpdateDebtCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new UpdateDebtCommandHandler(_repositoryMock, _capitalRepositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenDebtExists_ReturnsSuccess()
    {
        // Arrange
        var debt = new Debt(1)
        {
            Item = "Laptop",
            Amount = 1000m,
            Currency = CurrencyType.USD,
            Source = "John",
            Recipient = "Me",
            BorrowedAt = DateTimeOffset.UtcNow
        };
        var command = new UpdateDebtCommand(
            1, "Updated Item", 2000m, nameof(CurrencyType.EUR),
            "Jane", "You", DateTimeOffset.UtcNow,
            null, "Updated note", true, null);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        debt.Item.Should().Be("Updated Item");
        debt.Amount.Should().Be(2000m);
        debt.IsPaid.Should().BeTrue();

        _repositoryMock.Received(1).Update(debt);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDebtNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateDebtCommand(
            99, "Item", 100m, nameof(CurrencyType.UAH),
            "A", "B", DateTimeOffset.UtcNow, null, null, false, null);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns((Debt?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("debt"));

        _repositoryMock.DidNotReceive().Update(Arg.Any<Debt>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOnlyAmountChanges_AndIsPaidNotProvided_SavesChange()
    {
        // Arrange
        var debt = CreateDebt();
        var command = new UpdateDebtCommand(1, Amount: 1500m);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        debt.Amount.Should().Be(1500m);

        _repositoryMock.Received(1).Update(debt);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNothingChanges_DoesNotSave()
    {
        // Arrange
        var debt = CreateDebt();
        var command = new UpdateDebtCommand(1);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.DidNotReceive().Update(Arg.Any<Debt>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBecomingPaidFromCapital_DeductsDebtAmountFromCapital()
    {
        // Arrange
        var debt = CreateDebt();
        var capital = new Capital(5) { Name = "Wallet", Balance = 500m, Currency = CurrencyType.USD };
        var command = new UpdateDebtCommand(1, IsPaid: true, PayFromCapitalId: 5);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        debt.IsPaid.Should().BeTrue();
        capital.Balance.Should().Be(400m);

        _capitalRepositoryMock.Received(1).Update(capital);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyPaid_DoesNotDeductAgain()
    {
        // Arrange
        var debt = CreateDebt();
        debt.IsPaid = true;
        var command = new UpdateDebtCommand(1, Note: "changed", IsPaid: true, PayFromCapitalId: 5);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _capitalRepositoryMock.DidNotReceive().Update(Arg.Any<Capital>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Handle_WhenNoteIsEmpty_ClearsNote(string? note)
    {
        // Arrange
        var debt = CreateDebt();
        debt.Note = "Old note";

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);

        // Act
        var result = await _handler.Handle(new UpdateDebtCommand(1, Note: note), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        debt.Note.Should().BeNull();

        _repositoryMock.Received(1).Update(debt);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Debt CreateDebt() => new(1)
    {
        Item = "Laptop",
        Amount = 100m,
        Currency = CurrencyType.USD,
        Source = "John",
        Recipient = "Me",
        BorrowedAt = DateTimeOffset.UtcNow
    };
}
