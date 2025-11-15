using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Deed.Application.Categories.Commands.UpdateRange;

internal sealed class UpdateCategoriesCommandValidator : AbstractValidator<UpdateCategoriesCommand>

{
    public UpdateCategoriesCommandValidator()
    {
        RuleFor(c => c.Requests)
            .Must(requests => requests.Any())
            .WithError(DomainErrors.General.EmptyCollection)
            .Must(requests => !requests.Any(x => x.PeriodAmount < 0))
            .WithError(DomainErrors.Category.PeriodAmountGreaterEqualZero);
    }
}
