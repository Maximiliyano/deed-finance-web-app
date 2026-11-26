using Deed.Application.BankConnections.Commands.ImportBankHistory;
using Deed.Domain.Providers;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ImportBankHistoryCommandValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly ImportBankHistoryCommandValidator _validator;

    public ImportBankHistoryCommandValidatorTests()
    {
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(Now);

        _validator = new ImportBankHistoryCommandValidator(clock);
    }

    [Fact]
    public void Validate_PeriodWithinTenYears_PassesValidation()
    {
        // Arrange
        var command = new ImportBankHistoryCommand(Now.AddYears(-9), Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_StartOlderThanTenYears_FailsValidation()
    {
        // Arrange
        var command = new ImportBankHistoryCommand(Now.AddYears(-11));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.From);
    }

    [Fact]
    public void Validate_EndBeforeStart_FailsValidation()
    {
        // Arrange
        var command = new ImportBankHistoryCommand(Now.AddDays(-10), Now.AddDays(-20));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.To);
    }

    [Fact]
    public void Validate_OpenEndedPeriod_PassesValidation()
    {
        // Arrange
        var command = new ImportBankHistoryCommand(Now.AddDays(-30));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
