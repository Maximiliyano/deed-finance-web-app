using Deed.Application.BankNotifications.Queries.GetSummary;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class GetBankTransactionsSummaryQueryValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly GetBankTransactionsSummaryQueryValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("day")]
    [InlineData("Month")]
    [InlineData("YEAR")]
    public void Validate_KnownPeriod_PassesValidation(string? period)
    {
        var result = _validator.TestValidate(new GetBankTransactionsSummaryQuery(period));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_UnknownPeriod_FailsValidation()
    {
        var result = _validator.TestValidate(new GetBankTransactionsSummaryQuery("week"));

        result.ShouldHaveValidationErrorFor(q => q.Period);
    }

    [Fact]
    public void Validate_StartAfterEnd_FailsValidation()
    {
        var result = _validator.TestValidate(new GetBankTransactionsSummaryQuery(From: Now, To: Now.AddDays(-1)));

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_RangeLongerThanTenYears_FailsValidation()
    {
        var result = _validator.TestValidate(new GetBankTransactionsSummaryQuery(From: Now.AddYears(-11), To: Now));

        result.ShouldHaveAnyValidationError();
    }
}
