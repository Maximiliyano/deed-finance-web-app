using FluentValidation;

namespace Deed.Application.BankConnections.Commands.UpdateBankConnection;

internal sealed class UpdateBankConnectionCommandValidator : AbstractValidator<UpdateBankConnectionCommand>
{
    public UpdateBankConnectionCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0);

        RuleFor(c => c.Label)
            .MaximumLength(64);

        RuleFor(c => c.CapitalId)
            .GreaterThan(0)
            .When(c => c.CapitalId is not null);

        RuleFor(c => c.CapitalId)
            .NotNull()
            .When(c => c.AutoImport)
            .WithMessage("Link a capital to enable automatic adding.");

        RuleFor(c => c.ExpenseCategoryId)
            .NotNull()
            .When(c => c.AutoImport)
            .WithMessage("Choose an expense category for automatic adding.");

        RuleFor(c => c.IncomeCategoryId)
            .NotNull()
            .When(c => c.AutoImport)
            .WithMessage("Choose an income category for automatic adding.");
    }
}
