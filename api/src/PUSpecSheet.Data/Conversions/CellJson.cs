using System.Text.Json;
using System.Text.Json.Serialization;

namespace PUSpecSheet.Data.Conversions;

/// <summary>How cell configurations and styles are written to their JSON columns.</summary>
internal static class CellJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Options)
            ?? throw new InvalidOperationException($"The stored JSON \"{json}\" isn't a {typeof(T).Name}.");
    }
}
