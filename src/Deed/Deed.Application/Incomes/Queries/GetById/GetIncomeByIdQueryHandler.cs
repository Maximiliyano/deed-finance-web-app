using Deed.Application.Abstractions.Messaging;
using Deed.Application.Incomes.Responses;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Incomes.Queries.GetById;

internal sealed class GetIncomeByIdQueryHandler(
    IIncomeRepository repository)
    : IQueryHandler<GetIncomeByIdQuery, IncomeResponse>
{
    public async Task<Result<IncomeResponse>> Handle(GetIncomeByIdQuery query, CancellationToken cancellationToken)
    {
        var income = await repository.GetAsync(new IncomeByIdSpecification(query.Id)).ConfigureAwait(false);

        if (income is null)
        {
            return Result.Failure<IncomeResponse>(DomainErrors.General.NotFound(nameof(income)));
        }

        var incomeResponse = income.ToResponse();

        return Result.Success(incomeResponse);
    }
}
