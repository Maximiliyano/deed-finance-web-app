﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Exchanges.Service;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Tests.Common.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Deed.Tests.Unit.Exchanges.Services;

public sealed class ExchangeHttpServiceTests
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ILogger<ExchangeHttpService> _logger = Substitute.For<ILogger<ExchangeHttpService>>();
    private readonly IOptions<WebUrlSettings> _options = Options.Create(new WebUrlSettings
    {
        UIUrl = "https://ui.ex.com",
        ExchangeRatesPrivatAPIUrl = "https://api.ex.com/rates?date={0}"
    });

    private readonly DateTime _fixedNow = new(2024, 12, 1);

    public ExchangeHttpServiceTests()
    {
        _dateTimeProvider.UtcNow.Returns(_fixedNow);
    }

    private HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handler = new MockHttpMessageHandler((_, _) => Task.FromResult(response));
        return new HttpClient(handler);
    }

    [Fact]
    public async Task GetCurrencyAsync_ReturnsSuccess_WhenApiResponseIsValid()
    {
        // Arrange
        var json = """
        {
          "exchangeRate": [
            {
              "baseCurrency": "UAH",
              "currency": "USD",
              "purchaseRateNB": 36.0,
              "saleRateNB": 36.5
            }
          ]
        }
        """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var service = new ExchangeHttpService(_dateTimeProvider, _logger, _options, CreateHttpClient(response));

        // Act
        var result = await service.GetCurrencyAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.First().TargetCurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task GetCurrencyAsync_UsesFallbackRates_WhenPurchaseOrSaleIsNull()
    {
        // Arrange
        var json = """
        {
          "exchangeRate": [
            {
              "baseCurrency": "UAH",
              "currency": "EUR",
              "purchaseRateNB": 39.1,
              "saleRateNB": 39.5
            }
          ]
        }
        """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var service = new ExchangeHttpService(_dateTimeProvider, _logger, _options, CreateHttpClient(response));

        // Act
        var result = await service.GetCurrencyAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.First().Buy.Should().Be(39.1f);
        result.Value.First().Sale.Should().Be(39.5f);
    }

    [Fact]
    public async Task GetCurrencyAsync_ReturnsFailure_WhenResponseFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var service = new ExchangeHttpService(_dateTimeProvider, _logger, _options, CreateHttpClient(response));

        var result = await service.GetCurrencyAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.Exchange.HttpExecution);
    }

    [Fact]
    public async Task GetCurrencyAsync_ReturnsFailure_WhenResponseIsInvalidJson()
    {
        var invalidJson = "{ this is not valid json";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(invalidJson)
        };

        var service = new ExchangeHttpService(_dateTimeProvider, _logger, _options, CreateHttpClient(response));

        var result = await service.GetCurrencyAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.Exchange.HttpExecution); // because it throws in deserialization
    }

    [Fact]
    public async Task GetCurrencyAsync_ReturnsFailure_WhenDeserializationReturnsNull()
    {
        var nullResponse = "null";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(nullResponse)
        };

        var service = new ExchangeHttpService(_dateTimeProvider, _logger, _options, CreateHttpClient(response));

        var result = await service.GetCurrencyAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.Exchange.Serialization);
    }

    [Fact]
    public async Task GetCurrencyAsync_ReturnsFailure_WhenExceptionThrown()
    {
        var handler = new MockHttpMessageHandler((_, _) => throw new HttpRequestException("fail!"));
        var client = new HttpClient(handler);

        var service = new ExchangeHttpService(_dateTimeProvider, _logger, _options, client);

        var result = await service.GetCurrencyAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.Exchange.HttpExecution);
    }

    [Fact]
    public async Task GetCurrencyAsync_BuildsUrlWithCorrectDate()
    {
        string? requestedUrl = null;

        var handler = new MockHttpMessageHandler((req, _) =>
        {
            requestedUrl = req.RequestUri!.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "exchangeRate": []
                }
                """, Encoding.UTF8, "application/json")
            });
        });

        var client = new HttpClient(handler);
        var service = new ExchangeHttpService(_dateTimeProvider, _logger, _options, client);

        await service.GetCurrencyAsync();

        requestedUrl.Should().Be("https://api.ex.com/rates?date=01.12.2024");
    }
}
