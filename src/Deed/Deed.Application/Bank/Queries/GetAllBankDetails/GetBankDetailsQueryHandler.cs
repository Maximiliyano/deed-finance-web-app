using System.Net.Http.Json;
using System.Text.Json;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Bank.Responses;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.Bank.Queries.GetAllBankDetails;

internal sealed class GetBankDetailsQueryHandler(
    HttpClient client,
    IOptions<BankSettings> options)
    : IQueryHandler<GetBankDetailsQuery, BankDetailsResponse>
{
    private static readonly JsonSerializerOptions CaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public async Task<Result<BankDetailsResponse>> Handle(GetBankDetailsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("Sending request to get currencies...");

            client.DefaultRequestHeaders.Add("X-Token", options.Value.ApiKey);
            
            using var request = new HttpRequestMessage(HttpMethod.Get, options.Value.PersonalClientInfo);
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("HTTP request failed with status {Status}: {Reason}", (int)response.StatusCode, response.ReasonPhrase);
                return Result.Failure<BankDetailsResponse>(DomainErrors.HttpClient.Execution);
            }

            Log.Information("Deserializing a response...");
            var bankDetailsResponse = await response.Content.ReadFromJsonAsync<BankDetailsResponse>(CaseInsensitive, cancellationToken);

            if (bankDetailsResponse is null)
            {
                Log.Warning("Failed to deserialize response");
                return Result.Failure<BankDetailsResponse>(DomainErrors.HttpClient.Serialization);
            }
            
            Log.Information("Bank successfully retrieved.");
            
            return Result.Success(bankDetailsResponse);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Exception occurred while fetching bank details");
            return Result.Failure<BankDetailsResponse>(DomainErrors.HttpClient.Execution);
        }
    }
}
