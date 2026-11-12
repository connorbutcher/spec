using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// The settings to choose on the sheet for one cell, such as where a linked dropdown takes its choices
/// from. <see cref="Settings"/> must be for the cell's kind; null, or settings with nothing set, clears them.
/// </summary>
public sealed record CellSettingsRequest
{
    public CellSettingsRequest(int sheetCellId, CellInstanceSettings? settings)
    {
        this.SheetCellId = sheetCellId;
        this.Settings = settings;
    }

    public int SheetCellId { get; init; }

    public CellInstanceSettings? Settings { get; init; }
}
