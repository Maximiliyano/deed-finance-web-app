using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BudgetEstimations.Commands.Create;

internal sealed class CreateBudgetEstimationCommandHandler(
    IUser user,
    IBudgetEstimationRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBudgetEstimationCommand, int>
{
    public async Task<Result<int>> Handle(CreateBudgetEstimationCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await repository.CountAsync(
                new BudgetEstimationsByUserSpecification(user.Name!, false), cancellationToken).ConfigureAwait(false);

            if (count >= AuthConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var estimation = command.ToEntity();

        repository.Create(estimation);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("BudgetEstimation {Id} created", estimation.Id);

        return Result.Success(estimation.Id);
    }
}
