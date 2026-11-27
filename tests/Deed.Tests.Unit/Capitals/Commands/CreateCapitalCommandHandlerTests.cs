using Deed.Application.Auth;
using Deed.Application.Capitals.Commands.Create;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class CreateCapitalCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly ICapitalRepository _repositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateCapitalCommandHandler _handler;

    public CreateCapitalCommandHandlerTests()
    {
        _userMock.IsAuthenticated.Returns(true);
        _handler = new CreateCapitalCommandHandler(_userMock, _repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateValidCapital_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateCapitalCommand("FancyName  ", 1000, nameof(CurrencyType.USD), true, false);
        var exceptedCapital = new Capital(1)
        {
            Name = "FancyName",
            Balance = 1000,
            Currency = CurrencyType.USD,
            IncludeInTotal = true,
            OnlyForSavings = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _repositoryMock.Received(1).Create(Arg.Is<Capital>(c =>
            c.Name.Equals(exceptedCapital.Name, StringComparison.Ordinal) &&
            c.Balance.Equals(exceptedCapital.Balance) &&
            c.Currency.Equals(exceptedCapital.Currency) &&
            c.IncludeInTotal.Equals(exceptedCapital.IncludeInTotal) &&
            c.OnlyForSavings.Equals(exceptedCapital.OnlyForSavings)
        ));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AnonymousUserBelowLimit_ReturnsSuccess()
    {
        // Arrange
        _userMock.IsAuthenticated.Returns(false);
        _userMock.Id.Returns(AuthConstants.DefaultUserName);
        _repositoryMock
            .CountAsync(Arg.Any<UserCreatedCapitalsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(AuthConstants.EntityLimit - 1);

        var command = new CreateCapitalCommand("Wallet", 100, nameof(CurrencyType.USD), true, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Create(Arg.Any<Capital>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AnonymousUserAtLimit_ReturnsLimitReached()
    {
        // Arrange
        _userMock.IsAuthenticated.Returns(false);
        _userMock.Id.Returns(AuthConstants.DefaultUserName);
        _repositoryMock
            .CountAsync(Arg.Any<UserCreatedCapitalsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(AuthConstants.EntityLimit);

        var command = new CreateCapitalCommand("Wallet", 100, nameof(CurrencyType.USD), true, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.Anonymous.LimitReached);

        _repositoryMock.DidNotReceive().Create(Arg.Any<Capital>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
