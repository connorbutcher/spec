using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>Settings chosen for a cell of an earlier row, which a new row of the same kind takes on.</summary>
public sealed record ChosenSettings
{
    public ChosenSettings(int sheetRowRevisionId, CellInstanceSettings settings, int templateCellId, int? sheetColumnBlockId)
    {
        this.SheetRowRevisionId = sheetRowRevisionId;
        this.Settings = settings;
        this.TemplateCellId = templateCellId;
        this.SheetColumnBlockId = sheetColumnBlockId;
    }

    public int SheetRowRevisionId { get; init; }

    public CellInstanceSettings Settings { get; init; }

    public int TemplateCellId { get; init; }

    public int? SheetColumnBlockId { get; init; }
}
