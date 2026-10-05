namespace PUSpecSheet.Application.Published;

/// <summary>
/// A cell with the lookup key that held the value at the moment searched: which sheet it is on, and where
/// in the sheet. <see cref="IsHeader"/> is true for a cell in its table's header section.
/// </summary>
public sealed record PublishedLookupHit(
    int SheetId,
    Guid SheetPublicId,
    string PhaseCode,
    int SheetTypeId,
    int TableId,
    int SectionId,
    int RowId,
    int CellId,
    int? ColumnBlockId,
    bool IsHeader);
