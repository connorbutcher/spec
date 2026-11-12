using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Database.Conversions;

/// <summary>
/// Stores a <see cref="CellConfiguration"/> as JSON, with <c>kind</c> naming the derived type. The
/// configurations are immutable records, so EF's default equality-based change tracking works.
/// </summary>
internal sealed class CellConfigurationConverter
    : ValueConverter<CellConfiguration, string>
{
    public CellConfigurationConverter()
        : base(configuration => CellJson.Serialize(configuration),
        json => CellJson.Deserialize<CellConfiguration>(json))
    {
    }
}
