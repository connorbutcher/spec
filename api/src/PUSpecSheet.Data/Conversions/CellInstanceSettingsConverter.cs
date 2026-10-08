using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Data.Conversions;

/// <summary>
/// Stores a <see cref="CellInstanceSettings"/> as JSON, with <c>kind</c> naming the derived type. The
/// settings are immutable records, so EF's default equality-based change tracking works.
/// </summary>
internal sealed class CellInstanceSettingsConverter()
    : ValueConverter<CellInstanceSettings, string>(
        settings => CellJson.Serialize(settings),
        json => CellJson.Deserialize<CellInstanceSettings>(json));
