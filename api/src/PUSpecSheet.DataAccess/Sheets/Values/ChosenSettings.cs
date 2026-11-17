using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;

namespace PUSpecSheet.DataAccess.Sheets.Values;

/// <summary>Settings chosen for a cell of an earlier row, which a new row of the same kind takes on.</summary>
public sealed record ChosenSettings
{
    public required int SheetRowRevisionId { get; init; }

    public required CellInstanceSettings Settings { get; init; }

    public required int TemplateCellId { get; init; }

    public int? SheetColumnBlockId { get; init; }
}
