using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.UtilityBills.Commands.Update;

internal sealed class UpdateUtilityBillCommandHandler(
    IUtilityBillRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateUtilityBillCommand>
{
    public async Task<Result> Handle(UpdateUtilityBillCommand command, CancellationToken cancellationToken)
    {
        var bill = await repository
            .GetAsync(new UtilityBillByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (bill is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("utility bill"));
        }

        CurrencyType updatedCurrency = Enum.Parse<CurrencyType>(command.Currency);
        if (bill.Name == command.Name.Trim() &&
            bill.EstimatedAmount == command.EstimatedAmount &&
            bill.Currency == updatedCurrency &&
            bill.DueDayOfMonth == command.DueDayOfMonth &&
            bill.CapitalId == command.CapitalId &&
            bill.CategoryId == command.CategoryId &&
            bill.IsActive == command.IsActive)
        {
            return Result.Success();
        }
        
        bill.Name = command.Name.Trim();
        bill.EstimatedAmount = command.EstimatedAmount;
        bill.Currency = updatedCurrency;
        bill.DueDayOfMonth = command.DueDayOfMonth;
        bill.CapitalId = command.CapitalId;
        bill.CategoryId = command.CategoryId;
        bill.IsActive = command.IsActive;

        repository.Update(bill);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("UtilityBill {Id} updated", command.Id);

        return Result.Success();
    }
}
