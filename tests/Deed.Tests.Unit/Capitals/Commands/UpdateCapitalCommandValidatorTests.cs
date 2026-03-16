using Deed.Application.Capitals.Commands.Update;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Entities;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class UpdateCapitalCommandValidatorTests
{
    private readonly ICapitalRepository _repository = Substitute.For<ICapitalRepository>();
    private readonly UpdateCapitalCommandValidator _validator;

    public UpdateCapitalCommandValidatorTests()
    {
        _repository.AnyAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(false);
        _validator = new UpdateCapitalCommandValidator(_repository);
    }

    private static UpdateCapitalCommand ValidCommand => new(1);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NegativeBalance_FailsValidation()
    {
        var command = ValidCommand with { Balance = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Balance);
    }

    [Fact]
    public async Task Validate_ZeroBalance_PassesValidation()
    {
        var command = ValidCommand with { Balance = 0m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Balance);
    }

    [Fact]
    public async Task Validate_NameExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Name = new string('a', 33) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public async Task Validate_NameAlreadyExists_FailsValidation()
    {
        _repository.AnyAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(true);
        var command = ValidCommand with { Name = "Existing" };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { Currency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }

    [Fact]
    public async Task Validate_ValidCurrency_PassesValidation()
    {
        var command = ValidCommand with { Currency = CurrencyType.USD };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Currency);
    }
}
