using System.Net;
using Deed.Application.Abstractions.Caching;
using Deed.Application.Auth.Picture;
using Deed.Tests.Common.Mocks;
using FluentAssertions;

namespace Deed.Tests.Unit.Auth;

public sealed class ProfilePictureServiceTests
{
    private static readonly Uri Source = new("https://lh3.googleusercontent.com/a/photo");
    private static readonly byte[] Bytes = [1, 2, 3, 4];

    private readonly FakeCache _cache = new();

    [Fact]
    public async Task GetAsync_NotCached_FetchesOnceAndCaches()
    {
        // Arrange
        var calls = 0;
        var service = CreateService((_, _) =>
        {
            calls++;
            return Task.FromResult(Image());
        });

        // Act
        var result = await service.GetAsync(Source, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Data.ToArray().Should().Equal(Bytes);
        result.ContentType.Should().Be("image/jpeg");
        calls.Should().Be(1);
        _cache.Lifetimes[CacheKeys.ProfilePicture(Source)].Should().Be(TimeSpan.FromHours(24));

        var second = await service.GetAsync(Source, CancellationToken.None);

        second.Should().NotBeNull();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task GetAsync_TooManyRequests_ReturnsNullAndCachesMissBriefly()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)));

        // Act
        var result = await service.GetAsync(Source, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _cache.Lifetimes[CacheKeys.ProfilePicture(Source)].Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task GetAsync_NotAnImage_ReturnsNull()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html></html>", System.Text.Encoding.UTF8, "text/html")
        }));

        // Act
        var result = await service.GetAsync(Source, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_NonHttpsSource_DoesNotCallHttp()
    {
        // Arrange
        var calls = 0;
        var service = CreateService((_, _) =>
        {
            calls++;
            return Task.FromResult(Image());
        });

        // Act
        var result = await service.GetAsync(new Uri("http://example.com/a.jpg"), CancellationToken.None);

        // Assert
        result.Should().BeNull();
        calls.Should().Be(0);
    }

    private ProfilePictureService CreateService(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        var handler = new MockHttpMessageHandler(sendAsync);
#pragma warning disable CA2000
        var client = new HttpClient(handler);
#pragma warning restore CA2000
        handler.Dispose();

        return new ProfilePictureService(client, _cache);
    }

    private static HttpResponseMessage Image()
    {
        var content = new ByteArrayContent(Bytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private sealed class FakeCache : ICacheService
    {
        private readonly Dictionary<string, object?> _values = [];

        public Dictionary<string, TimeSpan> Lifetimes { get; } = [];

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) =>
            Task.FromResult(_values.TryGetValue(key, out var value) ? (T?)value : default);

        public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
        {
            _values[key] = value;
            Lifetimes[key] = expiration;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }
    }
}
