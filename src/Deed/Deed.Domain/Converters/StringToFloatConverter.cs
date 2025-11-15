using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deed.Domain.Converters;

public sealed class StringTofloatConverter : JsonConverter<float>
{
    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && float.TryParse(reader.GetString(), out float value))
        {
            return value;
        }

        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to {typeof(float)}.");
    }

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        double roundedValue = Math.Round(value, 2);

        writer.WriteStringValue(roundedValue.ToString(CultureInfo.InvariantCulture));
    }
}
