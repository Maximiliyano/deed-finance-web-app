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

    private readonly GetUserSettingsQueryHandler _handler;

    public GetUserSettingsQueryHandlerTests()
    {
        _handler = new GetUserSettingsQueryHandler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_WhenSettingsExist_ReturnsResponse()
    {
        // Arrange
        var name = "testName";
        var query = new GetUserSettingsQuery(name);
        var settings = new DomainUserSettings(1) { Currency = CurrencyType.USD };
        _repositoryMock.GetAsync(name, Arg.Any<CancellationToken>()).Returns(settings);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task Handle_WhenSettingsNotExist_ReturnsNull()
    {
        // Arrange
        var name = "testName";
        var query = new GetUserSettingsQuery(name);
        _repositoryMock.GetAsync(name, Arg.Any<CancellationToken>()).Returns((DomainUserSettings?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
