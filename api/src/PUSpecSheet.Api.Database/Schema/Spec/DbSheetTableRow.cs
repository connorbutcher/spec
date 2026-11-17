using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A row of a sheet table group.
/// </summary>
public record DbSheetTableRow
{
    public required int Id { get; init; }

    public required int SheetTableGroupId { get; init; }

    public required int TableRowTemplateId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbTableRowTemplate? TableRowTemplate { get; init; }

    public virtual ICollection<DbSheetTableRowRevision> Revisions { get; init; } = [];

    public virtual ICollection<DbSheetTableCell> Cells { get; init; } = [];
}
