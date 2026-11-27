using Deed.Application.UtilityBills;
using FluentAssertions;

namespace Deed.Tests.Unit.UtilityBills;

public sealed class UtilityBillMonthsTests
{
    [Fact]
    public void Previous_WhenMidYear_ReturnsPreviousCalendarMonth()
    {
        // Arrange
        var date = new DateTimeOffset(2026, 6, 14, 10, 30, 0, TimeSpan.Zero);

        // Act
        var (from, to) = UtilityBillMonths.Previous(date);

        // Assert
        from.Should().Be(new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero));
        to.Should().Be(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Previous_WhenJanuary_ReturnsDecemberOfPreviousYear()
    {
        // Arrange
        var date = new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero);

        // Act
        var (from, to) = UtilityBillMonths.Previous(date);

        // Assert
        from.Should().Be(new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero));
        to.Should().Be(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Previous_WhenOffsetCrossesMonthBoundary_UsesUtcMonth()
    {
        // Arrange
        var date = new DateTimeOffset(2026, 7, 1, 1, 0, 0, TimeSpan.FromHours(3));

        // Act
        var (from, to) = UtilityBillMonths.Previous(date);

        // Assert
        from.Should().Be(new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero));
        to.Should().Be(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
