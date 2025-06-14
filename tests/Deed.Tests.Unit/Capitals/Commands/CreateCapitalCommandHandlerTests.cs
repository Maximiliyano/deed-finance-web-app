using Deed.Application.Capitals.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
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
        var command = new CreateCapitalCommand("FancyName", 1000, CurrencyType.USD);
        var capital = new Capital
        {
            Name = command.Name,
            Balance = command.Balance,
            Currency = command.Currency
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(capital.Id);

        _repositoryMock.Received(1).Create(Arg.Any<Capital>());

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
