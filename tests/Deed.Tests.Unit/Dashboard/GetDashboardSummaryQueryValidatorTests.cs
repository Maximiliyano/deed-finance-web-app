using Deed.Application.Dashboard.Queries.GetSummary;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.Dashboard;

public sealed class GetDashboardSummaryQueryValidatorTests
{
    private readonly GetDashboardSummaryQueryValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("UAH")]
    [InlineData("USD")]
    public void Validate_PassesValidation_ForValidCurrency(string? currency)
    {
        // Arrange
        // Act
        var result = _validator.TestValidate(new GetDashboardSummaryQuery(currency));

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_FailsValidation_WhenPeriodEndIsNotAfterPeriodStart()
    {
        // Arrange
        var start = DateTime.UtcNow;

        // Act
        var result = _validator.TestValidate(new GetDashboardSummaryQuery(PeriodStart: start, PeriodEnd: start));

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.PeriodEnd);
    }

    [Theory]
    [InlineData("None")]
    [InlineData("XXX")]
    [InlineData("")]
    public void Validate_FailsValidation_ForInvalidCurrency(string currency)
    {
        // Arrange
        // Act
        var result = _validator.TestValidate(new GetDashboardSummaryQuery(currency));

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.Currency);
    }
}
