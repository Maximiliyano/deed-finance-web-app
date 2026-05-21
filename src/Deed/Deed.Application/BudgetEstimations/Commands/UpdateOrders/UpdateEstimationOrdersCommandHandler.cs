using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BudgetEstimations.Commands.UpdateOrders;

internal sealed class UpdateEstimationOrdersCommandHandler(
    IBudgetEstimationRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateEstimationOrdersCommand>
{
    public async Task<Result> Handle(UpdateEstimationOrdersCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        await repository.UpdateOrderIndexesAsync([.. request.Estimations.Select(e => (e.Id, e.OrderIndex))], user.Name, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
