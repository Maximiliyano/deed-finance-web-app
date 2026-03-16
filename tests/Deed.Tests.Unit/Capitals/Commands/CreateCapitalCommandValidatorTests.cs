using Deed.Application.Auth;
using Deed.Application.Capitals.Commands.Create;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Entities;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class CreateCapitalCommandValidatorTests
{
    private readonly ICapitalRepository _repository = Substitute.For<ICapitalRepository>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly CreateCapitalCommandValidator _validator;

    public CreateCapitalCommandValidatorTests()
    {
        _user.Name.Returns("testuser");
        _repository.AnyAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(false);
        _validator = new CreateCapitalCommandValidator(_repository, _user);
    }

    private static CreateCapitalCommand ValidCommand => new(
        "Cash", 1000m, CurrencyType.UAH, true, false);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyName_FailsValidation(string? name)
    {
        var command = ValidCommand with { Name = name! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
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
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.Name);
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
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { Currency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }
}
