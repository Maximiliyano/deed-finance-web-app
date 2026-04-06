using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Queries.GetLatest;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Exchanges.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Deed.Tests.Unit.Exchanges.Queries;

public sealed class GetLatestExchangesHandlerTests
{
    private readonly IExchangeRepository _repositoryMock = Substitute.For<IExchangeRepository>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();
    private readonly IOptions<MemoryCacheSettings> _settings;

    private readonly GetLatestExchangeQueryHandler _handler;

    public GetLatestExchangesHandlerTests()
    {
        _settings = Options.Create(new MemoryCacheSettings
        {
            ExchangesTimespanInHours = 1
        });
        _handler = new GetLatestExchangeQueryHandler(_settings, _repositoryMock, _cacheServiceMock);
    }

    [Fact]
    public async Task Handle_CachesAndReturnsFromRepository_WhenCacheIsMissing()
    {
        // Arrange
        var query = new GetLatestExchangeQuery();
        var exchangeEntities = new List<Exchange>
        {
            new()
            {
                NationalCurrencyCode = "UAH",
                TargetCurrencyCode = "EUR",
                Buy = 39.1m,
                Sale = 39.8m,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _repositoryMock.GetAllAsync(new ExchangesByQuerySpecification()).Returns(exchangeEntities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().OnlyContain(c =>
            c.NationalCurrency == "UAH" &&
            c.TargetCurrency == "EUR" &&
            c.Buy.Equals(39.1m) &&
            c.Sale.Equals(39.8m));
    }

    [Fact]
    public async Task Handle_ReturnsCachedExchanges_WhenCacheIsPresent()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        var exchanges = new List<Exchange>
        {
            new()
            {
                NationalCurrencyCode = "USD",
                TargetCurrencyCode = "UAH",
                Buy = 32.1m,
                Sale = 31.9m,
                CreatedAt = utcNow
            },
            new()
            {
                NationalCurrencyCode = "EUR",
                TargetCurrencyCode = "UAH",
                Buy = 29.6m,
                Sale = 28.2m,
                CreatedAt = utcNow
            }
        };
        var responses = exchanges.ToResponses();
        var query = new GetLatestExchangeQuery();

        _repositoryMock.GetAllAsync(Arg.Any<ExchangesByQuerySpecification>()).Returns(exchanges);
        _cacheServiceMock.GetAsync<List<ExchangeResponse>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(exchanges.ToResponses().ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(responses);

        await _repositoryMock.DidNotReceive().GetAllAsync(Arg.Any<ExchangesByQuerySpecification>());
    }
}
