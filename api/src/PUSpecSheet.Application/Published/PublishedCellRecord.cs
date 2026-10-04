namespace PUSpecSheet.Application.Published;

/// <summary>
/// One cell of a row that was on the sheet at a version, with the row revision that holds its value.
/// <see cref="Caption"/> is only read when labels are asked for.
/// </summary>
public sealed record PublishedCellRecord(
    int RowRevisionId,
    int RowId,
    Guid RowPublicId,
    int SectionId,
    int RowOrder,
    int CellId,
    Guid CellPublicId,
    int? ColumnBlockId,
    int Column,
    string? Caption);
