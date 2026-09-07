using System.Net.Http.Json;

namespace Deed.Tests.Integration.Infrastructure;

[Collection("Integration")]
public abstract class IntegrationTest
{
    protected readonly HttpClient Client;

    private readonly string _endpointPath;

    protected IntegrationTest(DeedApiFactory factory, string endpointPath)
    {
        Client = factory.CreateClient();

        _endpointPath = endpointPath;
    }

    protected async Task<TResponse> ExecutePostAsync<TRequest, TResponse>(TRequest request, string? path = "")
        where TRequest : class
    {
        HttpResponseMessage httpResponseMessage = await Client.PostAsJsonAsync($"{_endpointPath}/{path}", request);

        httpResponseMessage.EnsureSuccessStatusCode();

        TResponse? response = await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();

        ArgumentNullException.ThrowIfNull(response);

        return response;
    }

    protected async Task<TResponse?> ExecuteGetAsync<TResponse>(string? path = "")
        where TResponse : class
    {
        HttpResponseMessage httpResponseMessage = await Client.GetAsync($"{_endpointPath}/{path}");

        httpResponseMessage.EnsureSuccessStatusCode();

        return await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();
    }
}
