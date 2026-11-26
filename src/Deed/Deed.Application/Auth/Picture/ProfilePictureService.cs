using Deed.Application.Abstractions.Caching;
using Serilog;

namespace Deed.Application.Auth.Picture;

public sealed class ProfilePictureService(HttpClient client, ICacheService cache)
    : IProfilePictureService
{
    private const int MaxBytes = 1_048_576;

    private static readonly TimeSpan HitLifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan MissLifetime = TimeSpan.FromMinutes(10);

    public async Task<ProfilePicture?> GetAsync(Uri source, CancellationToken cancellationToken)
    {
        if (source.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        var key = CacheKeys.ProfilePicture(source);
        var cached = await cache.GetAsync<CachedPicture>(key, cancellationToken);

        if (cached is not null)
        {
            return cached.ToPicture();
        }

        var fetched = await FetchAsync(source, cancellationToken);

        await cache.SetAsync(
            key,
            fetched is null ? CachedPicture.Missing : CachedPicture.From(fetched),
            fetched is null ? MissLifetime : HitLifetime,
            cancellationToken);

        return fetched;
    }

    private async Task<ProfilePicture?> FetchAsync(Uri source, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Profile picture request failed with status {Status}", (int)response.StatusCode);
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;

            if (contentType is null || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (response.Content.Headers.ContentLength > MaxBytes)
            {
                return null;
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            return data.Length is > 0 and <= MaxBytes
                ? new ProfilePicture(data, contentType)
                : null;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Log.Warning(e, "Exception occurred while fetching the profile picture");
            return null;
        }
    }

    private sealed record CachedPicture(string? Base64, string? ContentType)
    {
        public static CachedPicture Missing { get; } = new(null, null);

        public static CachedPicture From(ProfilePicture picture) =>
            new(Convert.ToBase64String(picture.Data.Span), picture.ContentType);

        public ProfilePicture? ToPicture() =>
            Base64 is null || ContentType is null
                ? null
                : new ProfilePicture(Convert.FromBase64String(Base64), ContentType);
    }
}
