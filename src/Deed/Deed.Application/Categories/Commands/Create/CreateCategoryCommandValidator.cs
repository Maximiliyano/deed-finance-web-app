using Deed.Application.Categories.Specifications;
using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Categories.Commands.Create;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(ICategoryRepository repository)
    {
        RuleFor(c => c.Name)
            .MustAsync(async (name, _) => !await repository
                .AnyAsync(new CategoryByNameSpecification(name)).ConfigureAwait(false))
            .WithError(ValidationErrors.Category.AlreadyExists)
            .NotEmpty()
            .MaximumLength(ValidationConstants.MaxLenghtName);

        RuleFor(c => c.Type)
            .Must(type => type is not CategoryType.None)
            .WithError(ValidationErrors.Category.InvalidType);

        RuleFor(c => c.PlannedPeriodAmount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);
    }
};
