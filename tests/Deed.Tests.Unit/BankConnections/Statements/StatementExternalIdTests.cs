using Deed.Application.BankConnections.Statements;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Statements;

public sealed class StatementExternalIdTests
{
    private const string Iban = "UA213223130000026007233566001";

    private static readonly DateTimeOffset At = new(2026, 11, 26, 12, 0, 0, TimeSpan.FromHours(2));

    [Fact]
    public void Compute_SameInputs_ReturnTheSameId()
    {
        // Arrange
        // Act
        var first = StatementExternalId.Compute(Iban, At, -100m, "UAH", "Silpo", 500m);
        var second = StatementExternalId.Compute(Iban, At, -100m, "UAH", "Silpo", 500m);

        // Assert
        first.Should().Be(second);
        first.Should().StartWith("mail:").And.HaveLength(37);
    }

    [Fact]
    public void Compute_SameInstantInAnotherOffset_ReturnsTheSameId()
    {
        // Arrange
        // Act
        var local = StatementExternalId.Compute(Iban, At, -100m, "UAH", "Silpo", null);
        var utc = StatementExternalId.Compute(Iban, At.ToUniversalTime(), -100m, "UAH", "Silpo", null);

        // Assert
        local.Should().Be(utc);
    }

    [Fact]
    public void Compute_DescriptionWhitespaceAndCase_AreNormalized()
    {
        // Arrange
        // Act
        var first = StatementExternalId.Compute(Iban, At, -100m, "UAH", "Silpo   Kyiv", null);
        var second = StatementExternalId.Compute(Iban, At, -100m, "uah", " silpo kyiv ", null);

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void Compute_DifferentAmountTimeOrBalance_ReturnsDifferentIds()
    {
        // Arrange
        var baseline = StatementExternalId.Compute(Iban, At, -100m, "UAH", "Silpo", 500m);

        // Act
        var others = new[]
        {
            StatementExternalId.Compute(Iban, At, -101m, "UAH", "Silpo", 500m),
            StatementExternalId.Compute(Iban, At.AddMinutes(1), -100m, "UAH", "Silpo", 500m),
            StatementExternalId.Compute(Iban, At, -100m, "UAH", "Silpo", 400m),
            StatementExternalId.Compute("UA000000000000000000000000000", At, -100m, "UAH", "Silpo", 500m)
        };

        // Assert
        others.Should().NotContain(baseline);
        others.Distinct().Should().HaveCount(others.Length);
    }

    [Fact]
    public void WithOrdinal_FirstOccurrenceKeepsTheBaseId()
    {
        // Arrange
        var id = StatementExternalId.Compute(Iban, At, -100m, "UAH", "Silpo", null);

        // Act
        var ids = new[] { StatementExternalId.WithOrdinal(id, 0), StatementExternalId.WithOrdinal(id, 1), StatementExternalId.WithOrdinal(id, 2) };

        // Assert
        ids[0].Should().Be(id);
        ids.Distinct().Should().HaveCount(3);
        ids.Should().OnlyContain(value => value.Length <= 128);
    }
}
