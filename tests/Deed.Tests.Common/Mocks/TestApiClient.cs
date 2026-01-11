
using System.Net.Http.Json;
using System.Text.Json;

namespace Deed.Tests.Common.Mocks;

public sealed class TestApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private const string AppsettingsJson = "appsettings.Test.json";

    public TestApiClient()
    {
        var path = Directory.GetCurrentDirectory();
        var json = File.ReadAllText($"{path}/{AppsettingsJson}");
        var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonSerializerOptions);

        ArgumentNullException.ThrowIfNull(settings?.ApiUrl);

        var apiUrl = new Uri(settings.ApiUrl);

        _httpClient = new HttpClient()
        {
            BaseAddress = apiUrl
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

    public async ValueTask<bool> SendRequestAsync(HttpMethod method, Uri? uri = null)
    {
        using var request = new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = method,
        };

        var response = await _httpClient.SendAsync(request);

        if (response is null)
        {
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
