using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Published.Lookups;

/// <summary>
/// One cell read for a lookup match, with the row revision that holds its value: enough to place it, tell
/// a heading from a value, and name it by its lookup key.
/// </summary>
public sealed record PublishedLookupCellRecord
{
    public PublishedLookupCellRecord(
        int rowRevisionId,
        int rowId,
        Guid rowPublicId,
        int sectionId,
        int rowOrder,
        int cellId,
        Guid cellPublicId,
        int? columnBlockId,
        int column,
        CellKind kind,
        string? caption,
        string? lookupKey,
        bool isHeader)
    {
        RowRevisionId = rowRevisionId;
        RowId = rowId;
        RowPublicId = rowPublicId;
        SectionId = sectionId;
        RowOrder = rowOrder;
        CellId = cellId;
        CellPublicId = cellPublicId;
        ColumnBlockId = columnBlockId;
        Column = column;
        Kind = kind;
        Caption = caption;
        LookupKey = lookupKey;
        IsHeader = isHeader;
    }

    public int RowRevisionId { get; init; }

    public int RowId { get; init; }

    public Guid RowPublicId { get; init; }

    public int SectionId { get; init; }

    public int RowOrder { get; init; }

    public int CellId { get; init; }

    public Guid CellPublicId { get; init; }

    public int? ColumnBlockId { get; init; }

    public int Column { get; init; }

    public CellKind Kind { get; init; }

    public string? Caption { get; init; }

    public string? LookupKey { get; init; }

    public bool IsHeader { get; init; }
}
