using Deed.Application.Abstractions;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentValidation;

namespace Deed.Application.Expenses.Commands.Update;

internal sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator(
        ICategoryRepository categoryRepository,
        ICapitalRepository capitalRepository,
        IDateTimeProvider provider,
        IUser user)
    {
        RuleFor(i => i.Amount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero);

        RuleFor(e => e.Purpose)
            .MaximumLength(ValidationConstants.MaxLengthPurpose)
            .When(e => e.Purpose is not null);

        RuleForEach(e => e.TagNames)
            .MaximumLength(ValidationConstants.MaxLengthTagName)
            .When(e => e.TagNames is not null);

        RuleFor(e => e.CapitalId)
            .MustAsync(async (capitalId, ct) => await capitalRepository
                .AnyAsync(new CapitalByIdSpecification(capitalId!.Value, user.Name), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("capital"))
            .When(e => e.CapitalId.HasValue);

        RuleFor(e => e.CategoryId)
            .MustAsync(async (categoryId, ct) => await categoryRepository
                .AnyAsync(new CategoryByIdSpecification(categoryId!.Value), ct).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("category"))
            .MustAsync(async (categoryId, ct) => (await categoryRepository
                .GetAsync(new CategoryByIdSpecification(categoryId!.Value), ct).ConfigureAwait(false))?.Type == CategoryType.Expenses)
            .WithError(ValidationErrors.Category.InvalidType)
            .When(e => e.CategoryId.HasValue);

        RuleFor(i => i.Date)
            .Must(paymentDate => paymentDate!.Value.UtcDateTime != provider.MinValue)
            .LessThanOrEqualTo(provider.UtcNow)
            .WithError(ValidationErrors.Expense.InvalidPaymentDate)
            .When(e => e.Date.HasValue);

        RuleFor(e => e.Purpose)
            .Must(purpose => purpose?.Length is not 0 && !string.IsNullOrWhiteSpace(purpose))
            .WithError(ValidationErrors.Expense.PurposeEmptyOrWhitespace)
            .When(e => e.Purpose is not null);
    }
}
