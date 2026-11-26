using System.Text.Json;

namespace Deed.Application.BankConnections.Service;

public sealed record PrivatCardCredentials(string MerchantId, string Password)
{
    public string Serialize() => JsonSerializer.Serialize(this);

    public static PrivatCardCredentials? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var credentials = JsonSerializer.Deserialize<PrivatCardCredentials>(json);

            return credentials is { MerchantId.Length: > 0, Password.Length: > 0 } ? credentials : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
