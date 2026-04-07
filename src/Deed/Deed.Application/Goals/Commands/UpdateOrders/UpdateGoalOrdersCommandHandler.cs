using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Goals.Commands.UpdateOrders;

internal sealed class UpdateGoalOrdersCommandHandler(
    IGoalRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateGoalOrdersCommand>
{
    public async Task<Result> Handle(UpdateGoalOrdersCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        await repository.UpdateOrderIndexesAsync([.. request.Goals.Select(e => (e.Id, e.OrderIndex))], user.Name, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
