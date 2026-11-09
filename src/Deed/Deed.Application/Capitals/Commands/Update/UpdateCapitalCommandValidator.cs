using Deed.Application.Capitals.Specifications;
using Deed.Application.Abstractions;
using Deed.Application.Auth;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Capitals.Commands.Update;

internal sealed class UpdateCapitalCommandValidator : AbstractValidator<UpdateCapitalCommand>
{
    public UpdateCapitalCommandValidator(ICapitalRepository repository, IUser user)
    {
        RuleFor(c => c.Balance)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.Name)
            .MustAsync(async (command, name, ct) => !await repository
                .AnyAsync(new CapitalByNameSpecification(name!.Trim(), user.Name, command.Id), ct).ConfigureAwait(false))
            .When(c => !string.IsNullOrEmpty(c.Name))
            .WithError(ValidationErrors.General.NameAlreadyExists)
            .MaximumLength(ValidationConstants.MaxLengthName);

        RuleFor(c => c.Currency)
            .Must(c => Enum.TryParse<CurrencyType>(c, out var pc) && !pc.Equals(CurrencyType.None))
            .When(c => !string.IsNullOrEmpty(c.Currency))
            .WithError(ValidationErrors.General.InvalidCurrencyType);
    }
}
