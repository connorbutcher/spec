namespace PUSpecSheet.DataAccess.Published.Lookups;

/// <summary>
/// A cell with the lookup key that held the value at the moment searched: which sheet it is on, and where
/// in the sheet. <see cref="IsHeader"/> is true for a cell in its table's header section.
/// </summary>
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
