using Deed.Application.Capitals.Specifications;
using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Capitals.Commands.Create;

internal sealed class CreateCapitalCommandValidator : AbstractValidator<CreateCapitalCommand>
{
    public CreateCapitalCommandValidator(ICapitalRepository repository)
    {
        RuleFor(c => c.Name)
            .MustAsync(async (name, _) => !await repository
                .AnyAsync(new CapitalByNameSpecification(name)).ConfigureAwait(false))
            .WithError(ValidationErrors.Capital.AlreadyExists)
            .NotEmpty()
            .MaximumLength(ValidationConstants.MaxLenghtName);

        RuleFor(c => c.Balance)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero);

        RuleFor(c => c.Currency)
            .Must(currency => currency != CurrencyType.None)
            .WithError(ValidationErrors.Capital.InvalidCurrencyType);
    }
}
