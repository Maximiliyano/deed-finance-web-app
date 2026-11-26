using Deed.Application.BankConnections.Commands.ConnectPrivat;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectPrivatCommandValidatorTests
{
    private readonly ConnectPrivatCommandValidator _validator = new();

    [Theory]
    [InlineData("UA213223130000026007233566001")]
    [InlineData("ua21 3223 1300 0002 6007 2335 6600 1")]
    public void Validate_ValidIban_PassesValidation(string iban)
    {
        var result = _validator.TestValidate(new ConnectPrivatCommand("token", iban));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("UA123")]
    [InlineData("PL213223130000026007233566001")]
    [InlineData("UA21322313000002600723356600!")]
    public void Validate_InvalidIban_FailsValidation(string iban)
    {
        var result = _validator.TestValidate(new ConnectPrivatCommand("token", iban));

        result.ShouldHaveValidationErrorFor(c => c.Iban);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyToken_FailsValidation(string? token)
    {
        var result = _validator.TestValidate(new ConnectPrivatCommand(token!, "UA213223130000026007233566001"));

        result.ShouldHaveValidationErrorFor(c => c.Token);
    }

    [Fact]
    public void Validate_TooLongToken_FailsValidation()
    {
        var result = _validator.TestValidate(new ConnectPrivatCommand(new string('a', 513), "UA213223130000026007233566001"));

        result.ShouldHaveValidationErrorFor(c => c.Token);
    }
}
