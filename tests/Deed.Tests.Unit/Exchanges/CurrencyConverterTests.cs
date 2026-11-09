using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Responses;
using Deed.Domain.Enums;
using FluentAssertions;

namespace Deed.Tests.Unit.Exchanges;

public sealed class CurrencyConverterTests
{
    private static readonly ExchangeResponse[] Rates =
    [
        new("USD", "UAH", 41m, 42m, null),
        new("EUR", "UAH", 44m, 45m, null)
    ];

    private readonly CurrencyConverter _converter = new(Rates);

    [Fact]
    public void Convert_ReturnsSameAmount_WhenCurrenciesAreEqual()
    {
        // Arrange
        // Act
        var result = _converter.Convert(100m, CurrencyType.USD, CurrencyType.USD);

        // Assert
        result.Should().Be(100m);
    }

    [Fact]
    public void Convert_MultipliesBySale_WhenConvertingForeignToNational()
    {
        // Arrange
        // Act
        var result = _converter.Convert(10m, CurrencyType.USD, CurrencyType.UAH);

        // Assert
        result.Should().Be(420m);
    }

    [Fact]
    public void Convert_DividesByBuy_WhenConvertingNationalToForeign()
    {
        // Arrange
        // Act
        var result = _converter.Convert(410m, CurrencyType.UAH, CurrencyType.USD);

        // Assert
        result.Should().Be(10m);
    }

    [Fact]
    public void Convert_GoesThroughBaseCurrency_WhenConvertingBetweenForeignCurrencies()
    {
        // Arrange
        // Act
        var result = _converter.Convert(10m, CurrencyType.USD, CurrencyType.EUR);

        // Assert
        result.Should().Be(10m * 42m / 44m);
    }

    [Theory]
    [InlineData(CurrencyType.None, CurrencyType.UAH)]
    [InlineData(CurrencyType.UAH, CurrencyType.None)]
    public void Convert_ReturnsSameAmount_WhenCurrencyIsNone(CurrencyType from, CurrencyType to)
    {
        // Arrange
        // Act
        var result = _converter.Convert(5m, from, to);

        // Assert
        result.Should().Be(5m);
    }

    [Fact]
    public void Convert_ReturnsSameAmount_WhenRatesAreMissing()
    {
        // Arrange
        var converter = new CurrencyConverter([]);

        // Act
        var result = converter.Convert(5m, CurrencyType.USD, CurrencyType.UAH);

        // Assert
        result.Should().Be(5m);
    }

    [Fact]
    public void Convert_ReturnsSameAmount_WhenRateIsUnknown()
    {
        // Arrange
        // Act
        var result = _converter.Convert(5m, CurrencyType.PLN, CurrencyType.UAH);

        // Assert
        result.Should().Be(5m);
    }
}
