using Deed.Application.Transfers.Commands.Create;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.Transfers.Commands;

public sealed class CreateTransferCommandValidatorTests
{
    private readonly CreateTransferCommandValidator _validator = new();

    private static CreateTransferCommand ValidCommand => new(1, 2, 100m, 95m);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_NonPositiveAmount_FailsValidation(double amount)
    {
        var command = ValidCommand with { Amount = (decimal)amount };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Validate_NonPositiveDestinationAmount_FailsValidation(double amount)
    {
        var command = ValidCommand with { DestinationAmount = (decimal)amount };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.DestinationAmount);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(-1, 2)]
    public async Task Validate_InvalidCapitalIds_FailsValidation(int source, int destination)
    {
        var command = ValidCommand with { SourceCapitalId = source, DestinationCapitalId = destination };
        var result = await _validator.TestValidateAsync(command);
        Assert.False(result.IsValid);
    }
}
