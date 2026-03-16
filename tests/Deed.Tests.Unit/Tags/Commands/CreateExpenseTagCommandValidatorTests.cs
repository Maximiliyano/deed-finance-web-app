using Deed.Application.Tags.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Tags.Commands;

public sealed class CreateExpenseTagCommandValidatorTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly CreateExpenseTagCommandValidator _validator;

    public CreateExpenseTagCommandValidatorTests()
    {
        _tagRepository.AnyAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>()).Returns(false);
        _validator = new CreateExpenseTagCommandValidator(_tagRepository);
    }

    private static CreateExpenseTagCommand ValidCommand => new(1, "food");

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
        _tagRepository.AnyAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>()).Returns(true);
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}
