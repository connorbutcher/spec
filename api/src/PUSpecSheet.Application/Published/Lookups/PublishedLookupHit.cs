namespace PUSpecSheet.Application.Published.Lookups;

/// <summary>
/// A cell with the lookup key that held the value at the moment searched: which sheet it is on, and where
/// in the sheet. <see cref="IsHeader"/> is true for a cell in its table's header section.
/// </summary>
public sealed record PublishedLookupHit
{
    public PublishedLookupHit(
        int sheetId,
        Guid sheetPublicId,
        string phaseCode,
        int sheetTypeId,
        int tableId,
        int sectionId,
        int rowId,
        int cellId,
        int? columnBlockId,
        bool isHeader)
    {
        this.SheetId = sheetId;
        this.SheetPublicId = sheetPublicId;
        this.PhaseCode = phaseCode;
        this.SheetTypeId = sheetTypeId;
        this.TableId = tableId;
        this.SectionId = sectionId;
        this.RowId = rowId;
        this.CellId = cellId;
        this.ColumnBlockId = columnBlockId;
        this.IsHeader = isHeader;
    }

    public int SheetId { get; init; }

    public Guid SheetPublicId { get; init; }

    public string PhaseCode { get; init; }

    public int SheetTypeId { get; init; }

    public int TableId { get; init; }

    public int SectionId { get; init; }

    public int RowId { get; init; }

    public int CellId { get; init; }

    public int? ColumnBlockId { get; init; }

    public bool IsHeader { get; init; }
}
