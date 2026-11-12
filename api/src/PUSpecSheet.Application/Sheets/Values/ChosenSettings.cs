using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>Settings chosen for a cell of an earlier row, which a new row of the same kind takes on.</summary>
public sealed record ChosenSettings(int SheetRowRevisionId, CellInstanceSettings Settings, int TemplateCellId, int? SheetColumnBlockId);
