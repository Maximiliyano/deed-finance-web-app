using Deed.Application.Abstractions;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Constants;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;

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
                    .MaximumLength(ValidationConstants.MaxLengthName)
                    .WithError(ValidationErrors.Category.NameTooLong)
                    .MustAsync(async (req, name, ct) =>
                        !await categoryRepository.AnyAsync(new CategoryByNameSpecification(name!, req.Id), ct).ConfigureAwait(false))
                    .WithError(ValidationErrors.Category.AlreadyExists);
            });
    }
}
