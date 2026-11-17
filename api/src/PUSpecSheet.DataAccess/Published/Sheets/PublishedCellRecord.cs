namespace PUSpecSheet.DataAccess.Published.Sheets;

public sealed record PublishedCellRecord
{
    public required int RowRevisionId { get; init; }

    public required int RowId { get; init; }

    public required Guid RowPublicId { get; init; }

    public required int SectionId { get; init; }

    public required int RowOrder { get; init; }

    public required int CellId { get; init; }

    public required Guid CellPublicId { get; init; }

    public int? ColumnBlockId { get; init; }

    public required int Column { get; init; }

    public string? Caption { get; init; }
}
