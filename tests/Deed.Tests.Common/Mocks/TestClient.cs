using System.Net.Http.Json;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Tests.Common.Mocks;

public sealed class TestClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public TestClient()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:8000/")
        };
    }

    public async Task<T?> SendRequestAsync<T>(HttpMethod method, Uri? uri = null)
    {
        using var request = new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = method,
        };

        var response = await _httpClient.SendAsync(request);
        if (response is null)
        {
            return default;
        }
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TestResult> SendRequestAsync(HttpMethod method, Uri? uri = null)
    {
        using var request = new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = method,
        };

        var response = await _httpClient.SendAsync(request);
        if (response is null)
        {
            return TestResult.Failure();
        }
        var result = await response.Content.ReadFromJsonAsync<T>();
        if (result is null)
        {
            return new TestResult(false);
        }
        return new TestResult(true);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
