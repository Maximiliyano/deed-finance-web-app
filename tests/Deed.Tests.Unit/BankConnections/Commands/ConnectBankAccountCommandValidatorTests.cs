using Deed.Application.BankConnections.Commands.ConnectBankAccount;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectBankAccountCommandValidatorTests
{
    private readonly ConnectBankAccountCommandValidator _validator = new();

    [Theory]
    [InlineData(1, null)]
    [InlineData(7, "Savings")]
    public void Validate_ValidCommand_PassesValidation(int id, string? label)
    {
        // Arrange
        var command = new ConnectBankAccountCommand(id, label);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Validate_NonPositiveId_FailsValidation(int id)
    {
        // Arrange
        var command = new ConnectBankAccountCommand(id, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_TooLongLabel_FailsValidation()
    {
        // Arrange
        var command = new ConnectBankAccountCommand(1, new string('a', 65));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Label);
    }
}
