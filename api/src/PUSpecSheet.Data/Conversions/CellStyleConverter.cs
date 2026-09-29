using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Data.Conversions;

/// <summary>Stores a <see cref="CellStyle"/> as JSON. Styles are immutable records, so EF's default change tracking works.</summary>
internal sealed class CellStyleConverter()
    : ValueConverter<CellStyle, string>(
        style => CellJson.Serialize(style),
        json => CellJson.Deserialize<CellStyle>(json));
