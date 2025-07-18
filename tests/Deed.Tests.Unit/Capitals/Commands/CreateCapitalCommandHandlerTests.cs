using Deed.Application.Capitals.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class CreateCapitalCommandHandlerTests
{
    private readonly ICapitalRepository _repositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateCapitalCommandHandler _handler;

    public CreateCapitalCommandHandlerTests()
    {
        _handler = new CreateCapitalCommandHandler(_repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateValidCapital_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateCapitalCommand("FancyName  ", 1000, CurrencyType.USD);
        var exceptedCapital = new Capital(1)
        {
            Name = "FancyName",
            Balance = 1000,
            Currency = CurrencyType.USD
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _repositoryMock.Received(1).Create(Arg.Is<Capital>(c =>
            c.Name.Equals(exceptedCapital.Name, StringComparison.Ordinal) &&
            c.Balance.Equals(exceptedCapital.Balance) &&
            c.Currency.Equals(exceptedCapital.Currency)
        ));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
