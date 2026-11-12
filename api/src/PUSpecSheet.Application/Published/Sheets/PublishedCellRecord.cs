namespace PUSpecSheet.Application.Published.Sheets;

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
        this.RowRevisionId = rowRevisionId;
        this.RowId = rowId;
        this.RowPublicId = rowPublicId;
        this.SectionId = sectionId;
        this.RowOrder = rowOrder;
        this.CellId = cellId;
        this.CellPublicId = cellPublicId;
        this.ColumnBlockId = columnBlockId;
        this.Column = column;
        this.Caption = caption;
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
