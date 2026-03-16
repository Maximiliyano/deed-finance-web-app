using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Goals.Commands.Create;

internal sealed class CreateGoalCommandHandler(
    IGoalRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateGoalCommand, int>
{
    public async Task<Result<int>> Handle(CreateGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = command.ToEntity();

        repository.Create(goal);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Goal {Id} created", goal.Id);

        return Result.Success(goal.Id);
    }
}
