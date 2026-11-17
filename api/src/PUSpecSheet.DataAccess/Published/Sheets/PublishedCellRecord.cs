namespace PUSpecSheet.DataAccess.Published.Sheets;

/// <summary>
/// One cell of a row that was on the sheet at a version, with the row revision that holds its value.
/// <see cref="Caption"/> is only read when labels are asked for.
/// </summary>
public sealed record PublishedCellRecord
{
    public PublishedCellRecord(
        int rowRevisionId,
        int rowId,
        Guid rowPublicId,
        int sectionId,
        int rowOrder,
        int cellId,
        Guid cellPublicId,
        int? columnBlockId,
        int column,
        string? caption)
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
        Caption = caption;
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

    public string? Caption { get; init; }
}
