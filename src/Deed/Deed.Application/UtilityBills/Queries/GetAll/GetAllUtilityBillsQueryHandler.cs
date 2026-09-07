using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.UtilityBills.Responses;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.UtilityBills.Queries.GetAll;

internal sealed class GetAllUtilityBillsQueryHandler(IUtilityBillRepository repository, IUser user)
    : IQueryHandler<GetAllUtilityBillsQuery, IEnumerable<UtilityBillResponse>>
{
    public async Task<Result<IEnumerable<UtilityBillResponse>>> Handle(
        GetAllUtilityBillsQuery query,
        CancellationToken cancellationToken)
    {
        var bills = await repository
            .GetAllAsync(new UtilityBillsByUserSpecification(user.Name!, includeRelations: true, currentMonthPaymentsOnly: true), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(bills.ToResponses());
    }
}
