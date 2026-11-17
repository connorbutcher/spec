using System.Text.Json;

namespace PUSpecSheet.Api.Database.Serialization;

internal static class JsonSerializerSettings
{
    public static readonly JsonSerializerOptions SerializerSettings = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public static readonly JsonSerializerOptions DeserializerSettings = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
