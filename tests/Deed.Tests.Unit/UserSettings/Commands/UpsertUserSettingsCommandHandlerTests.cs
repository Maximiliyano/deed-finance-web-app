using Deed.Application.Auth;
using Deed.Application.UserSettings.Commands.Upsert;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;
using DomainUserSettings = Deed.Domain.Entities.UserSettings;

namespace Deed.Tests.Unit.UserSettings.Commands;

public sealed class UpsertUserSettingsCommandHandlerTests
{
    private readonly IUserSettingsRepository _repositoryMock = Substitute.For<IUserSettingsRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpsertUserSettingsCommandHandler _handler;

    public UpsertUserSettingsCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new UpsertUserSettingsCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenSettingsNotExist_CreatesNew()
    {
        // Arrange
        var command = new UpsertUserSettingsCommand(5000m, CurrencyType.UAH, false, null, false, null, false, null, false);

        _repositoryMock.GetAsync("testuser", Arg.Any<CancellationToken>())
            .Returns((DomainUserSettings?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _repositoryMock.Received(1).Create(Arg.Is<DomainUserSettings>(s =>
            s.Salary == 5000m &&
            s.Currency == CurrencyType.UAH
        ));
        _repositoryMock.DidNotReceive().Update(Arg.Any<DomainUserSettings>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSettingsExist_UpdatesExisting()
    {
        // Arrange
        var existing = new DomainUserSettings(1) { Salary = 3000m, Currency = CurrencyType.UAH };
        var command = new UpsertUserSettingsCommand(7000m, CurrencyType.USD, false, null, false, null, false, null, false);

        _repositoryMock.GetAsync("testuser", Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        existing.Salary.Should().Be(7000m);
        existing.Currency.Should().Be(CurrencyType.USD);

        _repositoryMock.DidNotReceive().Create(Arg.Any<DomainUserSettings>());
        _repositoryMock.Received(1).Update(existing);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
