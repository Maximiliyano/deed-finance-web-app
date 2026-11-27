using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.CancelBankHistory;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class CancelBankHistoryCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CancelBankHistoryCommandHandler _handler;

    public CancelBankHistoryCommandHandlerTests()
    {
        _userMock.Id.Returns("owner");

        _handler = new CancelBankHistoryCommandHandler(_userMock, _repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ImportInProgress_ClearsBackfillOfEveryConnection()
    {
        // Arrange
        var running = new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", BackfillFrom = Now.AddYears(-3), BackfillCursor = Now.AddDays(-30), BackfillFailures = 2 };
        var idle = new BankConnection(2) { EncryptedToken = "x", AccountIban = "UA2" };
        _repositoryMock
            .GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns([running, idle]);

        // Act
        var result = await _handler.Handle(new CancelBankHistoryCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        running.BackfillFrom.Should().BeNull();
        running.BackfillCursor.Should().BeNull();
        running.BackfillFailures.Should().Be(0);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NothingRunning_DoesNotSave()
    {
        // Arrange
        _repositoryMock
            .GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns([new BankConnection(2) { EncryptedToken = "x", AccountIban = "UA2" }]);

        // Act
        var result = await _handler.Handle(new CancelBankHistoryCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
