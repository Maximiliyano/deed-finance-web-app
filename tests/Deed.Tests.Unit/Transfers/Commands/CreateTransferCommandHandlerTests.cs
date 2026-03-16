using Deed.Application.Transfers.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Transfers.Commands;

public sealed class CreateTransferCommandHandlerTests
{
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly ITransferRepository _transferRepository = Substitute.For<ITransferRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateTransferCommandHandler _handler;

    public CreateTransferCommandHandlerTests()
    {
        _handler = new CreateTransferCommandHandler(_capitalRepository, _transferRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidTransfer_DeductsSourceAddsDestination()
    {
        // Arrange
        Capital source = new() { Name = "Cash", Balance = 1000m, Currency = CurrencyType.UAH };
        Capital dest = new() { Name = "Bank", Balance = 500m, Currency = CurrencyType.USD };
        _capitalRepository.GetAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns(source, dest);

        CreateTransferCommand command = new(1, 2, 100m, 2.5m);

        // Act
        Result<int> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.Balance.Should().Be(900m);
        dest.Balance.Should().Be(502.5m);
        _transferRepository.Received(1).Create(Arg.Any<Transfer>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SameSourceAndDest_ReturnsFailure()
    {
        // Arrange
        CreateTransferCommand command = new(1, 1, 100m, 100m);

        // Act
        Result<int> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Exchange.InvalidOperation);
    }

    [Fact]
    public async Task Handle_SourceNotFound_ReturnsFailure()
    {
        // Arrange
        _capitalRepository.GetAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns((Capital?)null);
        CreateTransferCommand command = new(99, 2, 100m, 100m);

        // Act
        Result<int> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DestNotFound_ReturnsFailure()
    {
        // Arrange
        Capital source = new() { Name = "Cash", Balance = 1000m, Currency = CurrencyType.UAH };
        _capitalRepository.GetAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns(source, (Capital?)null);
        CreateTransferCommand command = new(1, 99, 100m, 100m);

        // Act
        Result<int> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
