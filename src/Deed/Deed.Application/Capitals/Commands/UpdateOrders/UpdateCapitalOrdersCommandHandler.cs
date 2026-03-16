using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.UpdateOrders;

internal sealed class UpdateCapitalOrdersCommandHandler(
    ICapitalRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateCapitalOrdersCommand>
{
    public async Task<Result> Handle(UpdateCapitalOrdersCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        await repository.UpdateOrderIndexesAsync([.. request.Capitals.Select(c => (c.Id, c.OrderIndex))], user.Name, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
