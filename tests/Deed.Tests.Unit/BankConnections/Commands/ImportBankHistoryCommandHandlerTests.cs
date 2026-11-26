using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.ImportBankHistory;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ImportBankHistoryCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IUser _user = Substitute.For<IUser>();
    private readonly IBankTransactionSyncService _sync = Substitute.For<IBankTransactionSyncService>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ImportBankHistoryCommandHandler _sut;

    public ImportBankHistoryCommandHandlerTests()
    {
        _user.Name.Returns("owner");
        _clock.UtcNow.Returns(Now);
        _sut = new ImportBankHistoryCommandHandler(_user, _sync, _clock);
    }

    [Fact]
    public async Task Handle_NoEnd_ImportsUpToNow()
    {
        // Arrange
        var from = Now.AddYears(-1);
        _sync.ImportHistoryAsync("owner", from, Now, Arg.Any<CancellationToken>()).Returns(new BankHistoryImportResponse(5, true));

        // Act
        var result = await _sut.Handle(new ImportBankHistoryCommand(from), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new BankHistoryImportResponse(5, true));
    }

    [Fact]
    public async Task Handle_EndInFuture_IsClampedToNow()
    {
        // Arrange
        var from = Now.AddDays(-10);
        _sync.ImportHistoryAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new BankHistoryImportResponse(0, false));

        // Act
        await _sut.Handle(new ImportBankHistoryCommand(from, Now.AddDays(5)), CancellationToken.None);

        // Assert
        await _sync.Received(1).ImportHistoryAsync("owner", from, Now, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_StartNotBeforeEnd_Fails()
    {
        // Act
        var result = await _sut.Handle(new ImportBankHistoryCommand(Now.AddDays(1)), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.InvalidPeriod);
        await _sync.DidNotReceive().ImportHistoryAsync(
            Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }
}
