using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Deed.Application.Categories.Commands.UpdateRange;

internal sealed class UpdateCategoriesCommandValidator : AbstractValidator<UpdateCategoriesCommand>
{
    public UpdateCategoriesCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(c => c.Requests)
            .Cascade(CascadeMode.Stop)
            .Must(requests => requests.Any())
            .WithError(DomainErrors.General.EmptyCollection);

        RuleForEach(c => c.Requests)
            .ChildRules(request =>
            {
                request.RuleFor(r => r.PeriodAmount)
                    .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
                    .WithError(DomainErrors.Category.PeriodAmountGreaterEqualZero);

                request.RuleFor(r => r.Name)
                    .NotEmpty()
                    .WithError(ValidationErrors.Category.EmptyName)
                    .MaximumLength(ValidationConstants.MaxLenghtName)
                    .WithError(ValidationErrors.Category.NameTooLong)
                    .MustAsync(async (name, _) => // TODO fix bug, updateRange existing category - occuer error
                        !await categoryRepository.AnyAsync(new CategoryByNameSpecification(name)).ConfigureAwait(false))
                    .WithError(ValidationErrors.Category.AlreadyExists);
            });
    }
}
