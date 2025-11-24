using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Deed.Domain.Entities;

namespace Deed.Infrastructure.Persistence.DataSeed;

internal static class Seeder
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static IEnumerable<T> Parse<T>(string name)
        where T : Entity
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Persistence", "DataSeed", $"{name}.json");

        if (!File.Exists(filePath))
        {
            return [];
        }

        var text = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<IEnumerable<T>>(text, options) ?? [];
        return data;
    }
}
