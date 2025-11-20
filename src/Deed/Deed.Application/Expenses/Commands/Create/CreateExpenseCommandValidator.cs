using Deed.Application.Categories.Specifications;
using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentValidation;
using Deed.Domain.Providers;
using Deed.Application.Capitals.Specifications;

namespace Deed.Application.Expenses.Commands.Create;

internal sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator(
        ICategoryRepository categoryRepository,
        ICapitalRepository capitalRepository,
        IDateTimeProvider provider)
    {
        RuleFor(e => e.CapitalId)
            .MustAsync(async (capitalId, _) => await capitalRepository
                .AnyAsync(new CapitalByIdSpecification(capitalId)).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("capital"));

        RuleFor(e => e.CategoryId)
            .MustAsync(async (categoryId, _) => await categoryRepository
                .AnyAsync(new CategoryByIdSpecification(categoryId)).ConfigureAwait(false))
            .WithError(ValidationErrors.General.NotFound("category"))
            .MustAsync(async (categoryId, _) => (await categoryRepository
                .GetAsync(new CategoryByIdSpecification(categoryId)).ConfigureAwait(false))?.Type == CategoryType.Expenses)
            .WithError(ValidationErrors.Category.InvalidType);

        RuleFor(i => i.Amount)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue)
            .WithError(ValidationErrors.General.AmountMustBeGreaterThanZero);

        RuleFor(i => i.PaymentDate)
            .Must(paymentDate => paymentDate.UtcDateTime != provider.MinValue)
            .LessThanOrEqualTo(provider.UtcNow)
            .WithError(ValidationErrors.Expense.InvalidPaymentDate);

        RuleFor(i => i.Purpose)
            .Must(purpose => purpose?.Length is not 0 && !string.IsNullOrWhiteSpace(purpose))
            .WithError(ValidationErrors.Expense.PurposeEmptyOrWhitespace)
            .When(e => e.Purpose is not null);
    }
}
