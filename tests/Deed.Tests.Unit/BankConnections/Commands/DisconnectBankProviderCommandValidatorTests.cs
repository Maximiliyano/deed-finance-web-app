using Deed.Application.BankConnections.Commands.DisconnectBankProvider;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class DisconnectBankProviderCommandValidatorTests
{
    private readonly DisconnectBankProviderCommandValidator _validator = new();

    [Theory]
    [InlineData("PrivatAutoclient")]
    [InlineData("monobank")]
    [InlineData("MONOBANK")]
    public void Validate_KnownProvider_PassesValidation(string provider)
    {
        // Arrange
        var command = new DisconnectBankProviderCommand(provider);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("revolut")]
    public void Validate_UnknownProvider_FailsValidation(string provider)
    {
        // Arrange
        var command = new DisconnectBankProviderCommand(provider);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Provider);
    }
}
