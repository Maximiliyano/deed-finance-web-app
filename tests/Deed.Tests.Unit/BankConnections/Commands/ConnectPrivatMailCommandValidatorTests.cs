using Deed.Application.BankConnections.Commands.ConnectPrivatMail;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectPrivatMailCommandValidatorTests
{
    private const string Iban = "UA213223130000026007233566001";

    private readonly ConnectPrivatMailCommandValidator _validator = new();

    [Theory]
    [InlineData("UA213223130000026007233566001")]
    [InlineData("ua21 3223 1300 0002 6007 2335 6600 1")]
    public void Validate_ValidIban_PassesValidation(string iban)
    {
        // Arrange
        var command = new ConnectPrivatMailCommand(iban);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("UA123")]
    [InlineData("PL213223130000026007233566001")]
    [InlineData("UA21322313000002600723356600!")]
    public void Validate_InvalidIban_FailsValidation(string? iban)
    {
        // Arrange
        var command = new ConnectPrivatMailCommand(iban!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Iban);
    }

    [Theory]
    [InlineData("UAH")]
    [InlineData("usd")]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ValidCurrency_PassesValidation(string? currency)
    {
        // Arrange
        var command = new ConnectPrivatMailCommand(Iban, null, currency);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Currency);
    }

    [Theory]
    [InlineData("UA")]
    [InlineData("HRYVNA")]
    [InlineData("12A")]
    public void Validate_InvalidCurrency_FailsValidation(string currency)
    {
        // Arrange
        var command = new ConnectPrivatMailCommand(Iban, null, currency);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }

    [Fact]
    public void Validate_LabelLongerThanSixtyFourCharacters_FailsValidation()
    {
        // Arrange
        var command = new ConnectPrivatMailCommand(Iban, new string('x', 65));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Label);
    }
}
