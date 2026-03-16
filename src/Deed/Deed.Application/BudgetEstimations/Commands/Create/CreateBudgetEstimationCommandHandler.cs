using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BudgetEstimations.Commands.Create;

internal sealed class CreateBudgetEstimationCommandHandler(
    IBudgetEstimationRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBudgetEstimationCommand, int>
{
    public async Task<Result<int>> Handle(CreateBudgetEstimationCommand command, CancellationToken cancellationToken)
    {
        var estimation = command.ToEntity();

        repository.Create(estimation);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("BudgetEstimation {Id} created", estimation.Id);

        return Result.Success(estimation.Id);
    }
}
