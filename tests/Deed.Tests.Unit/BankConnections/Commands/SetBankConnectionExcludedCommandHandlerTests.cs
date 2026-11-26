using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.SetBankConnectionExcluded;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class SetBankConnectionExcludedCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly SetBankConnectionExcludedCommandHandler _handler;

    public SetBankConnectionExcludedCommandHandlerTests()
    {
        _userMock.Name.Returns("owner");

        _handler = new SetBankConnectionExcludedCommandHandler(_userMock, _repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_Exclude_FlagsConnectionAndStopsItsImport()
    {
        // Arrange
        var connection = new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", BackfillFrom = Now.AddYears(-1), BackfillCursor = Now.AddDays(-30), BackfillFailures = 3 };
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(connection);

        // Act
        var result = await _handler.Handle(new SetBankConnectionExcludedCommand(1, true), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        connection.IsExcluded.Should().BeTrue();
        connection.BackfillCursor.Should().BeNull();
        connection.BackfillFailures.Should().Be(0);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Include_ClearsTheFlag()
    {
        // Arrange
        var connection = new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", IsExcluded = true };
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(connection);

        // Act
        await _handler.Handle(new SetBankConnectionExcludedCommand(1, false), CancellationToken.None);

        // Assert
        connection.IsExcluded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NothingChanges_DoesNotSave()
    {
        // Arrange
        var connection = new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1" };
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(connection);

        // Act
        var result = await _handler.Handle(new SetBankConnectionExcludedCommand(1, false), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownConnection_ReturnsNotConnected()
    {
        // Arrange
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>()).Returns((BankConnection?)null);

        // Act
        var result = await _handler.Handle(new SetBankConnectionExcludedCommand(9, true), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.NotConnected);
    }
}
