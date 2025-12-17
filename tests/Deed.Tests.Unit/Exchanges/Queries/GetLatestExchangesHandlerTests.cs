using Deed.Application.Abstractions.Settings;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Queries.GetLatest;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Exchanges.Service;
using Deed.Application.Exchanges.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Deed.Tests.Unit.Exchanges.Queries;

public sealed class GetLatestExchangesHandlerTests
{
    private readonly IExchangeRepository _repositoryMock = Substitute.For<IExchangeRepository>();
    private readonly IMemoryCache _memoryCacheMock = Substitute.For<IMemoryCache>();
    private readonly IOptions<MemoryCacheSettings> _settings;

    private readonly GetLatestExchangeQueryHandler _handler;

    public GetLatestExchangesHandlerTests()
    {
        _settings = Options.Create(new MemoryCacheSettings
        {
            ExchangesTimespanInHours = 1
        });
        _handler = new GetLatestExchangeQueryHandler(_settings, _repositoryMock, _memoryCacheMock);
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
        _memoryCacheMock.Set(nameof(Exchange), exchanges, TimeSpan.FromHours(3));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(responses);

        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<ExchangesByQuerySpecification>());
    }
}
