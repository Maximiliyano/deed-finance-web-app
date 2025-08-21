using Deed.Application.Capitals.Commands.Update;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class UpdateCapitalCommandHandlerTests
{
    private readonly ICapitalRepository _repositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly UpdateCapitalCommandHandler _handler;

    public UpdateCapitalCommandHandlerTests()
    {
        _handler = new UpdateCapitalCommandHandler(_repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_GivenNonExistentCapital_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateCapitalCommand(1);

        _repositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns((Capital)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("capital"));

        _repositoryMock.DidNotReceive().Update(Arg.Any<Capital>());

        await _repositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCapitalHaveNothingToUpdate()
    {
        // Arrange
        const int id = 1;

        var capital = new Capital(id) { Name = "Test", Balance = 100, Currency = CurrencyType.UAH };
        var command = new UpdateCapitalCommand(id);

        _repositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Update(capital);

        await _repositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("New Name", 200f, CurrencyType.EUR)]
    [InlineData("Modified Name", 150f, null)]
    [InlineData(null, 150f, null)]
    [InlineData(null, null, CurrencyType.UAH)]
    [InlineData("Updated Name", null, CurrencyType.USD)]
    [InlineData("Refreshed Name", null, null)]
    public async Task Handle_ShouldUpdateCapitalSuccessfully(string? name, float? balance, CurrencyType? currency)
    {
        // Arrange
        const string oldName = "Old Name";
        const float oldBalance = 100f;

        var capital = new Capital(1) { Name = oldName, Balance = oldBalance, Currency = CurrencyType.USD };
        var command = new UpdateCapitalCommand(
            1,
            name,
            balance,
            currency);

        _repositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        capital.Name.Should().Be(name ?? oldName);
        capital.Balance.Should().Be(balance ?? oldBalance);
        capital.Currency.Should().Be(currency is not null
            ? currency
            : CurrencyType.USD);

        _repositoryMock.Received(1).Update(capital);

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCurrencyIsInvalid()
    {
        // Arrange
        var capital = new Capital(1) { Name = "Test", Balance = 100, Currency = CurrencyType.USD };
        var command = new UpdateCapitalCommand(1, "Test", 100, CurrencyType.None);
        _repositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e == DomainErrors.Capital.InvalidCurrency);
    }
}
