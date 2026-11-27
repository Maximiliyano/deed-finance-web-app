using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.UpdateBankConnection;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class UpdateBankConnectionCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly ICapitalRepository _capitalsMock = Substitute.For<ICapitalRepository>();
    private readonly ICategoryRepository _categoriesMock = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly UpdateBankConnectionCommandHandler _handler;
    private readonly BankConnection _connection = new(1) { EncryptedToken = "x", AccountIban = "UA1", Label = "Main" };

    public UpdateBankConnectionCommandHandlerTests()
    {
        _userMock.Id.Returns("owner");
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(_connection);
        _capitalsMock
            .GetAsync(Arg.Any<CapitalByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new Capital(3) { Name = "Wallet", Balance = 0m, Currency = CurrencyType.UAH });
        _categoriesMock
            .GetAsync(Arg.Any<CategoryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new Category(9) { Name = "Food", Type = CategoryType.Expenses });

        _handler = new UpdateBankConnectionCommandHandler(_userMock, _repositoryMock, _capitalsMock, _categoriesMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_UnknownConnection_ReturnsNotFound()
    {
        // Arrange
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>()).Returns((BankConnection?)null);

        // Act
        var result = await _handler.Handle(new UpdateBankConnectionCommand(1, null, null, false, null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NothingChanges_DoesNotSave()
    {
        // Arrange
        var command = new UpdateBankConnectionCommand(1, "Main", null, false, null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LinkCapitalAndCategories_UpdatesTheConnection()
    {
        // Arrange
        _categoriesMock
            .GetAsync(Arg.Any<CategoryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(call => new Category(9) { Name = "c", Type = CategoryType.Expenses }, call => new Category(10) { Name = "i", Type = CategoryType.Incomes });
        var command = new UpdateBankConnectionCommand(1, " Savings ", 3, true, 9, 10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _connection.Label.Should().Be("Savings");
        _connection.CapitalId.Should().Be(3);
        _connection.AutoImport.Should().BeTrue();
        _connection.ExpenseCategoryId.Should().Be(9);
        _connection.IncomeCategoryId.Should().Be(10);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownCapital_ReturnsNotFound()
    {
        // Arrange
        _capitalsMock.GetAsync(Arg.Any<CapitalByIdSpecification>(), Arg.Any<CancellationToken>()).Returns((Capital?)null);

        // Act
        var result = await _handler.Handle(new UpdateBankConnectionCommand(1, null, 3, false, null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _connection.CapitalId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExpenseCategoryOfTheWrongType_ReturnsMismatch()
    {
        // Arrange
        _categoriesMock
            .GetAsync(Arg.Any<CategoryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new Category(9) { Name = "Salary", Type = CategoryType.Incomes });

        // Act
        var result = await _handler.Handle(new UpdateBankConnectionCommand(1, null, 3, false, 9, null), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.CategoryTypeMismatch);
    }

    [Fact]
    public async Task Handle_UnlinkingTheCapital_TurnsAutoImportOff()
    {
        // Arrange
        _connection.CapitalId = 3;
        _connection.AutoImport = true;

        // Act
        await _handler.Handle(new UpdateBankConnectionCommand(1, null, null, true, null, null), CancellationToken.None);

        // Assert
        _connection.CapitalId.Should().BeNull();
        _connection.AutoImport.Should().BeFalse();
    }
}
