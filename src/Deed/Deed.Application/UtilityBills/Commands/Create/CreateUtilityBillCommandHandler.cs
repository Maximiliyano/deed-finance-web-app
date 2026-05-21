using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.UtilityBills.Commands.Create;

internal sealed class CreateUtilityBillCommandHandler(
    IUser user,
    IUtilityBillRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUtilityBillCommand, int>
{
    public async Task<Result<int>> Handle(CreateUtilityBillCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await repository.CountAsync(
                new UtilityBillsByUserSpecification(user.Name!), cancellationToken).ConfigureAwait(false);

            if (count >= AuthConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var bill = command.ToEntity();

        repository.Create(bill);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("UtilityBill {Id} created", bill.Id);

        return Result.Success(bill.Id);
    }
}
