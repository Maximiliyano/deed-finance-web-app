using Deed.Application.Auth;
using Deed.Application.UserSettings.Queries.Get;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using DomainUserSettings = Deed.Domain.Entities.UserSettings;

namespace Deed.Tests.Unit.UserSettings.Queries;

public sealed class GetUserSettingsQueryHandlerTests
{
    private readonly IUserSettingsRepository _repositoryMock = Substitute.For<IUserSettingsRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetUserSettingsQueryHandler _handler;

    public GetUserSettingsQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetUserSettingsQueryHandler(_repositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenSettingsExist_ReturnsResponse()
    {
        // Arrange
        var query = new GetUserSettingsQuery();
        var settings = new DomainUserSettings(1) { Salary = 5000m, Currency = CurrencyType.UAH };

        _repositoryMock.GetAsync("testuser", Arg.Any<CancellationToken>()).Returns(settings);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Salary.Should().Be(5000m);
        result.Value.Currency.Should().Be("UAH");
    }

    [Fact]
    public async Task Handle_WhenSettingsNotExist_ReturnsNull()
    {
        // Arrange
        var query = new GetUserSettingsQuery();
        _repositoryMock.GetAsync("testuser", Arg.Any<CancellationToken>()).Returns((DomainUserSettings?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
