using Deed.Application.Auth;
using Deed.Application.Transfers.Commands.Delete;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Transfers.Commands;

public sealed class DeleteTransferCommandHandlerTests
{
    private readonly ITransferRepository _transferRepository = Substitute.For<ITransferRepository>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly DeleteTransferCommandHandler _handler;

    public DeleteTransferCommandHandlerTests()
    {
        _user.Name.Returns("testuser");
        _handler = new DeleteTransferCommandHandler(_transferRepository, _capitalRepository, _unitOfWork, _user);
    }

    [Fact]
    public async Task Handle_WhenTransferExists_ReversesBalancesAndDeletes()
    {
        // Arrange
        Transfer transfer = new()
            { Amount = 100m, DestinationAmount = 250m, SourceCapitalId = 1, DestinationCapitalId = 2 };
        Capital source = new() { Name = "Cash", Balance = 900m, Currency = CurrencyType.UAH };
        Capital dest = new() { Name = "Bank", Balance = 750m, Currency = CurrencyType.USD };

        _transferRepository.GetAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns(transfer);
        _capitalRepository.GetAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns(source, dest);

        // Act
        Result result = await _handler.Handle(new DeleteTransferCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.Balance.Should().Be(1000m); // +100 reversed
        dest.Balance.Should().Be(500m); // -250 reversed
        _transferRepository.Received(1).Delete(transfer);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransferNotFound_ReturnsFailure()
    {
        // Arrange
        _transferRepository.GetAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns((Transfer?)null);

        // Act
        Result result = await _handler.Handle(new DeleteTransferCommand(99), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _transferRepository.DidNotReceive().Delete(Arg.Any<Transfer>());
    }
}
