using Deed.Application.BankNotifications.Queries.GetPage;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class GetBankTransactionsQueryValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly GetBankTransactionsQueryValidator _validator = new();

    [Fact]
    public void Validate_NoFilters_PassesValidation()
    {
        // Arrange
        var query = new GetBankTransactionsQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SearchOverTheLimit_FailsValidation()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Search: new string('a', 101));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.Search);
    }

    [Fact]
    public void Validate_PeriodEndingBeforeItStarts_FailsValidation()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(From: Now, To: Now.AddDays(-1));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q);
    }

    [Fact]
    public void Validate_OpenEndedPeriod_PassesValidation()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(From: Now);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
