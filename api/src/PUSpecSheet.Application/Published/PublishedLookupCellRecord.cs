using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// One cell read for a lookup match, with the row revision that holds its value: enough to place it, tell
/// a heading from a value, and name it by its lookup key.
/// </summary>
public sealed record PublishedLookupCellRecord(
    int RowRevisionId,
    int RowId,
    Guid RowPublicId,
    int SectionId,
    int RowOrder,
    int CellId,
    Guid CellPublicId,
    int? ColumnBlockId,
    int Column,
    CellKind Kind,
    string? Caption,
    string? LookupKey,
    bool IsHeader);
