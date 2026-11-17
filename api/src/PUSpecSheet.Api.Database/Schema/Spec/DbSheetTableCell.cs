using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A cell of a sheet table row, at one of its row template's cells. Its values are held per row revision.
/// </summary>
public record DbSheetTableCell
{
    public required int Id { get; init; }

    public required int SheetTableRowId { get; init; }

    public required int TableCellTemplateId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbTableCellTemplate? TableCellTemplate { get; init; }
}
