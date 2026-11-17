using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;

namespace PUSpecSheet.DataAccess.Sheets.Values;

public sealed record ChosenSettings
{
    public required int SheetRowRevisionId { get; init; }

    public required CellInstanceSettings Settings { get; init; }

    public required int TemplateCellId { get; init; }

    public int? SheetColumnBlockId { get; init; }
}
