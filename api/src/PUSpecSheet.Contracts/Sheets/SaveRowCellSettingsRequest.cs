using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Changes the settings chosen on the sheet for cells of one row. Like saving values, it locks the row to
/// its author until they publish or discard.
/// </summary>
public sealed record SaveRowCellSettingsRequest
{
    public SaveRowCellSettingsRequest([MinLength(1)] IReadOnlyList<CellSettingsRequest> settings)
    {
        this.Settings = settings;
    }

    public IReadOnlyList<CellSettingsRequest> Settings { get; init; }
}
