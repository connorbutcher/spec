namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetTableRow
{
    public required int Id { get; init; }

    public required int SheetTableGroupId { get; init; }

    public required int TableRowTemplateId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required SheetItemRevision Revision { get; init; }

    public required int DisplayOrder { get; init; }

    public required ICollection<SheetTableCell> Cells { get; init; } = [];
}
