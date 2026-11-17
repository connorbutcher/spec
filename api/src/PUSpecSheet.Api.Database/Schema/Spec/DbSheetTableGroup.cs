using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A group of a sheet table.
/// </summary>
public record DbSheetTableGroup
{
    public required int Id { get; init; }

    public required int SheetTableId { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbTableGroupTemplate? TableGroupTemplate { get; init; }

    public virtual ICollection<DbSheetTableGroupRevision> Revisions { get; init; } = [];

    public virtual ICollection<DbSheetTableRow> Rows { get; init; } = [];
}
