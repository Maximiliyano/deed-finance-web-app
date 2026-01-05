using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Application.Expenses.Specifications;
using Deed.Application.Tags.Specifications;
using Deed.Domain.Constants;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Tags.Commands.Create;

internal sealed class CreateExpenseTagCommandValidator : AbstractValidator<CreateExpenseTagCommand>
{
    public CreateExpenseTagCommandValidator(ITagRepository tagRepository)
    {
        RuleFor(x => x.Name)
            .MustAsync(async (name, _) => !await tagRepository
                .AnyAsync(new TagByNameSpecification(name)).ConfigureAwait(false))
            .WithError(ValidationErrors.Tag.AlreadyExists)
            .NotEmpty()
            .MaximumLength(ValidationConstants.MaxLenghtName);
    }
}
