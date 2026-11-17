using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Schema.Cell;

/// <summary>
/// A cell's own configuration, overriding its template's, in a row revision.
/// </summary>
public record DbSheetCellConfigurationOverride
{
    public required int SheetTableRowRevisionId { get; init; }

    public required int SheetTableCellId { get; init; }

    public required ICellProperties ConfigurationOverride { get; init; }

    public virtual DbSheetTableCell? SheetTableCell { get; init; }
}
