using Deed.Application.Abstractions.Messaging;
using Deed.Application.Bank.Responses;

namespace Deed.Application.Bank.Queries.GetAllBankDetails;

public sealed class GetBankDetailsQuery
    : IQuery<BankDetailsResponse>;
