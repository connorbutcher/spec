using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// Changes the settings chosen on the sheet for cells of one row. Like saving values, it locks the row to
/// its author until they publish it or change it back.
/// </summary>
public sealed record SaveRowCellSettingsRequest
{
    [MinLength(1)]
    public required IReadOnlyList<CellSettingsRequest> Settings { get; init; }
}
