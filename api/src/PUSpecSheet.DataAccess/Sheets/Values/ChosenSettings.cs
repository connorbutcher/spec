using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;

namespace PUSpecSheet.DataAccess.Sheets.Values;

/// <summary>Settings chosen for a cell of an earlier row, which a new row of the same kind takes on.</summary>
public sealed record ChosenSettings
{
    public ChosenSettings(int sheetRowRevisionId, CellInstanceSettings settings, int templateCellId, int? sheetColumnBlockId)
    {
        SheetRowRevisionId = sheetRowRevisionId;
        Settings = settings;
        TemplateCellId = templateCellId;
        SheetColumnBlockId = sheetColumnBlockId;
    }

    public int SheetRowRevisionId { get; init; }

    public CellInstanceSettings Settings { get; init; }

    public int TemplateCellId { get; init; }

    public int? SheetColumnBlockId { get; init; }
}
