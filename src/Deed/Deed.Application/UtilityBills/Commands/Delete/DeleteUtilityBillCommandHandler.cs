using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.UtilityBills.Commands.Delete;

internal sealed class DeleteUtilityBillCommandHandler(
    IUtilityBillRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<DeleteUtilityBillCommand>
{
    public async Task<Result> Handle(DeleteUtilityBillCommand command, CancellationToken cancellationToken)
    {
        var bill = await repository
            .GetAsync(new UtilityBillByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (bill is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("utility bill"));
        }

        repository.Delete(bill);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("UtilityBill {Id} deleted", command.Id);

        return Result.Success();
    }
}
