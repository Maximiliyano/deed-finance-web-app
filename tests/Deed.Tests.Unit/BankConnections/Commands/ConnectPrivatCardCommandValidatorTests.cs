using Deed.Application.BankConnections.Commands.ConnectPrivatCard;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectPrivatCardCommandValidatorTests
{
    private readonly ConnectPrivatCardCommandValidator _validator = new();

    [Theory]
    [InlineData("4111111111111111")]
    [InlineData("4111 1111 1111 1111")]
    public void Validate_ValidCard_PassesValidation(string card)
    {
        var result = _validator.TestValidate(new ConnectPrivatCardCommand("12345", "pass", card));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("4111")]
    [InlineData("4111111111111112")]
    public void Validate_InvalidCard_FailsValidation(string card)
    {
        var result = _validator.TestValidate(new ConnectPrivatCardCommand("12345", "pass", card));

        result.ShouldHaveValidationErrorFor(c => c.CardNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12a45")]
    public void Validate_InvalidMerchantId_FailsValidation(string merchantId)
    {
        var result = _validator.TestValidate(new ConnectPrivatCardCommand(merchantId, "pass", "4111111111111111"));

        result.ShouldHaveValidationErrorFor(c => c.MerchantId);
    }

    [Fact]
    public void Validate_EmptyPassword_FailsValidation()
    {
        var result = _validator.TestValidate(new ConnectPrivatCardCommand("12345", string.Empty, "4111111111111111"));

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }
}
