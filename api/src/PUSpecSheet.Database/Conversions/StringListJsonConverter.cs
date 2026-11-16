using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PUSpecSheet.Database.Conversions;

/// <summary>Stores a list of strings as a JSON array.</summary>
internal sealed class StringListJsonConverter
    : ValueConverter<List<string>?, string>
{
    public StringListJsonConverter()
        : base(
            list => JsonSerializer.Serialize(list!, (JsonSerializerOptions?)null),
            json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
    {
    }
}
