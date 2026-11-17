namespace PUSpecSheet.DataAccess.Published.Sheets;

/// <summary>
/// One cell of a row that was on the sheet at a version, with the row revision that holds its value.
/// <see cref="Caption"/> is only read when labels are asked for.
/// </summary>
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
