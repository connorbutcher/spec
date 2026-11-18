namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetTableGroup
{
    public required int Id { get; init; }

    public required int SheetTableId { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required SheetItemRevision Revision { get; init; }

    public required int DisplayOrder { get; init; }

    /// <summary>
    /// The ReferenceID of the group this one is nested under.
    /// </summary>
    public required Guid? LinkedGroupReferenceId { get; init; }

    public required ICollection<SheetTableRow> Rows { get; init; } = [];

    public required ICollection<SheetTableGroupWidget> Widgets { get; init; } = [];
}
