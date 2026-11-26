using Deed.Application.BankConnections.Commands.ConnectPrivat;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectPrivatCommandValidatorTests
{
    private const string Iban = "UA213223130000026007233566001";

    private readonly ConnectPrivatCommandValidator _validator = new();

    [Theory]
    [InlineData("UA213223130000026007233566001")]
    [InlineData("ua21 3223 1300 0002 6007 2335 6600 1")]
    public void Validate_ValidIban_PassesValidation(string iban)
    {
        // Arrange
        var command = new ConnectPrivatCommand("token", iban);

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
        var command = new ConnectPrivatCommand("token", iban!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Iban);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyToken_FailsValidation(string? token)
    {
        // Arrange
        var command = new ConnectPrivatCommand(token!, Iban);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Token);
    }

    [Fact]
    public void Validate_TooLongToken_FailsValidation()
    {
        // Arrange
        var command = new ConnectPrivatCommand(new string('a', 513), Iban);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Token);
    }
}
