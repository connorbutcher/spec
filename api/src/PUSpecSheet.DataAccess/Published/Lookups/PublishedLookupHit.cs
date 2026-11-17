namespace PUSpecSheet.DataAccess.Published.Lookups;

public sealed record PublishedLookupHit
{
    public required int SheetId { get; init; }

    public required Guid SheetPublicId { get; init; }

    public required string PhaseCode { get; init; }

    public required int SheetTypeId { get; init; }

    public required int TableId { get; init; }

    public required int SectionId { get; init; }

    public required int RowId { get; init; }

    public required int CellId { get; init; }

    public int? ColumnBlockId { get; init; }

    public required bool IsHeader { get; init; }
}
