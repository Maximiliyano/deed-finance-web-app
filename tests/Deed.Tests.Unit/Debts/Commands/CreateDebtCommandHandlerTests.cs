using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Commands.Create;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Debts.Commands;

public sealed class CreateDebtCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IDebtRepository _debtRepository = Substitute.For<IDebtRepository>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IExchangeRateProvider _ratesMock = Substitute.For<IExchangeRateProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateDebtCommandHandler _handler;

    public CreateDebtCommandHandlerTests()
    {
        _userMock.IsAuthenticated.Returns(true);
        _handler = new CreateDebtCommandHandler(_userMock, _debtRepository, _capitalRepository, _ratesMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateValidDebt_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateDebtCommand(
            "Laptop  ", 1500m, nameof(CurrencyType.USD),
            "John  ", "Me  ", DateTimeOffset.UtcNow,
            null, "  Some note  ", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _debtRepository.Received(1).Create(Arg.Is<Debt>(d =>
            d.Item == "Laptop" &&
            d.Amount == 1500m &&
            d.Currency == CurrencyType.USD &&
            d.Source == "John" &&
            d.Recipient == "Me" &&
            d.Note == "Some note"
        ));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCapitalCurrencyDiffers_CreditsConvertedAmount()
    {
        // Arrange
        var capital = new Capital(7) { Name = "Wallet", Balance = 1000m, Currency = CurrencyType.UAH };
        var command = new CreateDebtCommand(
            "Laptop", 100m, nameof(CurrencyType.USD), "John", "Me", DateTimeOffset.UtcNow, null, null, 7);

        _capitalRepository.GetAsync(Arg.Any<CapitalByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(capital);
        _ratesMock.GetLatestAsync(Arg.Any<CancellationToken>())
            .Returns([new ExchangeResponse("USD", "UAH", 41m, 42m, null)]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(5200m);
    }

    [Fact]
    public async Task Handle_WhenCapitalCurrencyMatches_CreditsAmountWithoutRateLookup()
    {
        // Arrange
        var capital = new Capital(7) { Name = "Wallet", Balance = 1000m, Currency = CurrencyType.USD };
        var command = new CreateDebtCommand(
            "Laptop", 100m, nameof(CurrencyType.USD), "John", "Me", DateTimeOffset.UtcNow, null, null, 7);

        _capitalRepository.GetAsync(Arg.Any<CapitalByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(1100m);

        await _ratesMock.DidNotReceive().GetLatestAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRateIsMissing_FailsWithoutChangingBalanceOrSaving()
    {
        // Arrange
        var capital = new Capital(7) { Name = "Wallet", Balance = 1000m, Currency = CurrencyType.UAH };
        var command = new CreateDebtCommand(
            "Laptop", 100m, nameof(CurrencyType.USD), "John", "Me", DateTimeOffset.UtcNow, null, null, 7);

        _capitalRepository.GetAsync(Arg.Any<CapitalByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(capital);
        _ratesMock.GetLatestAsync(Arg.Any<CancellationToken>()).Returns([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.Exchange.RateNotFound);
        capital.Balance.Should().Be(1000m);

        _debtRepository.DidNotReceive().Create(Arg.Any<Debt>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreateDebtWithNullNote_SetsNoteToNull()
    {
        // Arrange
        var command = new CreateDebtCommand(
            "Rent", 500m, nameof(CurrencyType.UAH),
            "Bank", "Me", DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMonths(1), null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _debtRepository.Received(1).Create(Arg.Is<Debt>(d =>
            d.Note == null &&
            d.DeadlineAt != null
        ));
    }
}
