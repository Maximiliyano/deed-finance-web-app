using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Debts.Commands.UpdateOrders;

internal sealed class UpdateDebtOrdersCommandHandler(
    IDebtRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateDebtOrdersCommand>
{
    public async Task<Result> Handle(UpdateDebtOrdersCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Id);

        await repository.UpdateOrderIndexesAsync([.. request.Debts.Select(e => (e.Id, e.OrderIndex))], user.Id, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
