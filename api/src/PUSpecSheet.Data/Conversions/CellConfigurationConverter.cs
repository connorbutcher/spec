using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Data.Conversions;

/// <summary>
/// Stores a <see cref="CellConfiguration"/> as JSON, with <c>kind</c> naming the derived type. The
/// configurations are immutable records, so EF's default equality-based change tracking works.
/// </summary>
internal sealed class CellConfigurationConverter()
    : ValueConverter<CellConfiguration, string>(
        configuration => CellJson.Serialize(configuration),
        json => CellJson.Deserialize<CellConfiguration>(json));
