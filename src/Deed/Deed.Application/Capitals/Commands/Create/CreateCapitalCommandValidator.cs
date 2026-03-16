using Deed.Application.Capitals.Specifications;
using Deed.Application.Abstractions;
using Deed.Application.Auth;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Capitals.Commands.Create;

internal sealed class CreateCapitalCommandValidator : AbstractValidator<CreateCapitalCommand>
{
    public CreateCapitalCommandValidator(ICapitalRepository repository, IUser user)
    {
        RuleFor(c => c.Name)
            .MustAsync(async (name, ct) => !await repository
                .AnyAsync(new CapitalByNameSpecification(name, user.Name), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.Capital.AlreadyExists)
            .NotEmpty()
            .MaximumLength(ValidationConstants.MaxLengthName);

        RuleFor(c => c.Balance)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero);

        RuleFor(c => c.Currency)
            .Must(currency => currency != CurrencyType.None)
            .WithError(ValidationErrors.Capital.InvalidCurrencyType);
    }
}
