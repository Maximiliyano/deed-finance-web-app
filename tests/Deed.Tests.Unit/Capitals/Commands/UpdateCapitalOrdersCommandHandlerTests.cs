using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Capitals.Commands.Update;
using Deed.Application.Capitals.Commands.UpdateOrders;
using Deed.Application.Capitals.Requests;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class UpdateCapitalOrdersCommandHandlerTests
{
    private readonly ICapitalRepository _repositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly UpdateCapitalOrdersCommandHandler _handler;

    public UpdateCapitalOrdersCommandHandlerTests()
    {
        _handler = new UpdateCapitalOrdersCommandHandler(_repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_Should_UpdateOrderIndexes_And_SaveChanges()
    {
        // Arrange
        var command = new UpdateCapitalOrdersCommand(new List<CapitalOrder>
        {
            new(1, 0),
            new(2, 1),
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _repositoryMock.Received(1)
            .UpdateOrderIndexes(Arg.Is<IEnumerable<(int, int)>>(x =>
                x.Any(c => c.Item2 == 0) &&
                x.Any(c => c.Item2 == 1)));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotThrow_When_EmptyList()
    {
        // Arrange
        var command = new UpdateCapitalOrdersCommand([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repositoryMock.Received(1).UpdateOrderIndexes(Arg.Any<IEnumerable<(int, int)>>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Pass_CancellationToken_To_SaveChanges()
    {
        // Arrange
        var command = new UpdateCapitalOrdersCommand([
            new (1, 5),
        ]);
        
        var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.Handle(command, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cts.Token);

        cts.Dispose();
    }
}
